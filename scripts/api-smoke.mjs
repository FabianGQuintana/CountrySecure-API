// Prueba de humo end-to-end de la API real (autorización por rol, paginación, baja lógica y flujos clave).
//
// Uso (con la API y la base levantadas):
//   SMOKE_PASSWORD_ADMIN=... SMOKE_PASSWORD_RESIDENT=... SMOKE_PASSWORD_SECURITY=... node scripts/api-smoke.mjs
// Opcionales: API_URL (def. http://localhost:5021), SMOKE_EMAIL_<ROL> (def. usuarios demo del seed).
//
// Deja la base como la encontró: lo que crea lo da de baja (baja lógica) al final.

const API = (process.env.API_URL || "http://localhost:5021").replace(/\/$/, "");
const USERS = {
  Admin: { email: process.env.SMOKE_EMAIL_ADMIN || "admin@countrysecure.local", password: process.env.SMOKE_PASSWORD_ADMIN },
  Resident: { email: process.env.SMOKE_EMAIL_RESIDENT || "residente@countrysecure.local", password: process.env.SMOKE_PASSWORD_RESIDENT },
  Security: { email: process.env.SMOKE_EMAIL_SECURITY || "seguridad@countrysecure.local", password: process.env.SMOKE_PASSWORD_SECURITY },
};

let failures = 0;
const check = (ok, label, extra = "") => {
  console.log(`  ${ok ? "✓" : "✗"} ${label}${extra ? `  (${extra})` : ""}`);
  if (!ok) failures++;
};

async function call(method, path, token, body) {
  const res = await fetch(API + path, {
    method,
    headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}), ...(body ? { "Content-Type": "application/json" } : {}) },
    body: body ? JSON.stringify(body) : undefined,
    signal: AbortSignal.timeout(30000),
  });
  let data = null;
  const text = await res.text();
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  return { status: res.status, data };
}

async function login(role) {
  const { email, password } = USERS[role];
  const r = await call("POST", "/api/auth/login", null, { email, password });
  if (r.status !== 200) throw new Error(`Login ${role} falló: ${r.status}`);
  return { token: r.data.accessToken, id: r.data.userId };
}

const isPaged = (d) => d && Array.isArray(d.items) && ["page", "pageSize", "totalCount", "totalPages"].every((k) => typeof d[k] === "number");
const suffix = Math.random().toString(36).slice(2, 7);
const dni = () => 20_000_000 + Math.floor(Math.random() * 39_000_000);

async function expectPaged(label, path, token) {
  const r = await call("GET", path, token);
  check(r.status === 200 && isPaged(r.data), `GET ${path}`, `${r.status}${r.status === 200 ? "" : " " + JSON.stringify(r.data).slice(0, 120)}`);
  return r;
}
async function expectStatus(path, token, status, method = "GET", body) {
  const r = await call(method, path, token, body);
  check(r.status === status, `${method} ${path} → ${status}`, `obtuve ${r.status}${r.status !== status ? " " + JSON.stringify(r.data).slice(0, 140) : ""}`);
  return r;
}

async function main() {
  for (const [role, u] of Object.entries(USERS)) if (!u.password) throw new Error(`Falta SMOKE_PASSWORD_${role.toUpperCase()}`);
  console.log(`API: ${API}\n`);

  const admin = await login("Admin");
  const resident = await login("Resident");
  const security = await login("Security");
  const created = { amenity: null, lot: null, order: null, user: null, visit: null, permit: null, turn: null, request: null, property: null };

  console.log("1) Sin token: todo protegido");
  await expectStatus("/api/amenities", null, 401);
  await expectStatus("/api/entry-permissions", null, 401);
  await expectStatus("/api/users", null, 401);

  console.log("\n2) Admin: listados paginados con formato único");
  for (const p of ["/api/users", "/api/users?includeInactive=true", "/api/lots", "/api/properties", "/api/amenities", "/api/orders",
    "/api/requests", "/api/turns", "/api/visits", "/api/entry-permissions", "/api/entry-permissions/today", "/api/entry-permissions/history"])
    await expectPaged(p, p, admin.token);
  await expectStatus("/api/lots/block-names", admin.token, 200);
  await expectStatus("/api/stats", admin.token, 200);
  const big = await call("GET", "/api/users?pageSize=1000", admin.token);
  check(big.data?.pageSize === 100, "pageSize se acota a 100", String(big.data?.pageSize));
  const small = await call("GET", "/api/users?page=1&pageSize=2", admin.token);
  check(small.data?.items?.length <= 2 && small.data?.totalPages >= 1, "paginación en la base (pageSize=2)", `items=${small.data?.items?.length} total=${small.data?.totalCount}`);

  console.log("\n3) Residente: solo lo suyo");
  for (const p of ["/api/entry-permissions/me", "/api/turns/me", "/api/requests/me", "/api/properties/me", "/api/orders", "/api/amenities", "/api/visits"])
    await expectPaged(p, p, resident.token);
  await expectStatus("/api/users/me", resident.token, 200);
  await expectStatus("/api/stats", resident.token, 200);
  await expectStatus("/api/users", resident.token, 403);
  await expectStatus("/api/lots", resident.token, 403);
  await expectStatus("/api/properties", resident.token, 403);
  await expectStatus("/api/entry-permissions/today", resident.token, 403);
  await expectStatus(`/api/users/${security.id}`, resident.token, 403);
  await expectStatus("/api/amenities", resident.token, 403, "POST", { amenityName: "x", description: "x", schedules: "x", capacity: 1 });

  console.log("\n4) Seguridad: portería sí, reservas no");
  for (const p of ["/api/entry-permissions/today", "/api/entry-permissions/history", "/api/visits", "/api/orders"]) await expectPaged(p, p, security.token);
  await expectStatus("/api/stats", security.token, 200);
  await expectStatus("/api/turns", security.token, 403);
  await expectStatus("/api/requests", security.token, 403);
  await expectStatus("/api/entry-permissions/validate?code=no-existe", security.token, 404);

  console.log("\n5) Baja lógica y restauración (Amenity, Lot, Order)");
  {
    const a = await call("POST", "/api/amenities", admin.token, { amenityName: `Smoke ${suffix}`, description: "prueba", schedules: "9-18", capacity: 2 });
    check(a.status === 201, "crear amenity", String(a.status)); created.amenity = a.data?.id;
    const dup = await call("POST", "/api/amenities", admin.token, { amenityName: `Smoke ${suffix}`, description: "prueba", schedules: "9-18", capacity: 2 });
    check(dup.status === 409, "nombre duplicado → 409", String(dup.status));
    const bad = await call("POST", "/api/amenities", admin.token, { amenityName: "", description: "", schedules: "", capacity: 0 });
    check(bad.status === 400, "datos inválidos → 400", String(bad.status));
    const del = await call("DELETE", `/api/amenities/${created.amenity}`, admin.token);
    check(del.status === 200 && del.data.status === "Inactive", "DELETE da de baja (status Inactive)", String(del.status));
    const hidden = await call("GET", `/api/amenities/${created.amenity}`, admin.token);
    check(hidden.status === 404, "el filtro global la oculta (404)", String(hidden.status));
    const list = await call("GET", `/api/amenities?search=Smoke ${suffix}`, admin.token);
    check(list.data?.totalCount === 0, "no aparece en el listado", String(list.data?.totalCount));
    const withInactive = await call("GET", `/api/amenities?search=Smoke ${suffix}&includeInactive=true`, admin.token);
    check(withInactive.data?.totalCount === 1, "includeInactive=true (Admin) la incluye", String(withInactive.data?.totalCount));
    const asResident = await call("GET", `/api/amenities?search=Smoke ${suffix}&includeInactive=true`, resident.token);
    check(asResident.data?.totalCount === 0, "includeInactive lo ignora un residente", String(asResident.data?.totalCount));
    const back = await call("POST", `/api/amenities/${created.amenity}/restore`, admin.token);
    check(back.status === 200 && back.data.status === "Active", "restore la reactiva", String(back.status));
    await call("DELETE", `/api/amenities/${created.amenity}`, admin.token);

    const lot = await call("POST", "/api/lots", admin.token, { lotName: `Smoke-${suffix}`, blockName: "Z", lotState: "Available" });
    check(lot.status === 201, "crear lote", String(lot.status)); created.lot = lot.data?.lotId;
    const lotDel = await call("DELETE", `/api/lots/${created.lot}`, admin.token);
    check(lotDel.status === 200 && lotDel.data.lotState === "Inactive", "baja de lote → estado Inactive", String(lotDel.status));
    const lotDup = await call("POST", "/api/lots", admin.token, { lotName: `Smoke-${suffix}`, blockName: "Z", lotState: "Available" });
    check(lotDup.status === 409, "nombre de lote dado de baja sigue reservado → 409", String(lotDup.status));

    const order = await call("POST", "/api/orders", admin.token, { description: "prueba", supplierName: `Prov ${suffix}`, orderType: "Cleaning" });
    check(order.status === 201 && order.data.orderType === "Cleaning", "crear servicio (enum como texto)", String(order.status)); created.order = order.data?.id;
  }

  console.log("\n6) Usuarios: alta, baja (revoca sesión) y restauración");
  {
    const email = `smoke.${suffix}@countrysecure.local`;
    const reg = await call("POST", "/api/auth/register", admin.token, { name: "Smoke", lastname: "Test", dni: dni(), phone: "3794111111", email, password: "Smoke1234", role: "Resident" });
    check(reg.status === 201, "register (solo Admin)", String(reg.status)); created.user = reg.data?.userId;
    check((await call("POST", "/api/auth/register", resident.token, { name: "X", lastname: "Y", dni: dni(), phone: "3794111111", email: `x${suffix}@x.com`, password: "Smoke1234", role: "Admin" })).status === 403, "un residente no puede registrar usuarios");
    const l1 = await call("POST", "/api/auth/login", null, { email, password: "Smoke1234" });
    check(l1.status === 200, "el usuario nuevo puede entrar");
    const deact = await call("DELETE", `/api/users/${created.user}`, admin.token);
    check(deact.status === 200 && deact.data.active === false, "dar de baja", String(deact.status));
    const refresh = await call("POST", "/api/auth/refresh", null, { accessToken: l1.data.accessToken, refreshToken: l1.data.refreshToken });
    check(refresh.status === 401, "su refresh token queda revocado", String(refresh.status));
    check((await call("POST", "/api/auth/login", null, { email, password: "Smoke1234" })).status === 401, "no puede iniciar sesión");
    const self = await call("DELETE", `/api/users/${admin.id}`, admin.token);
    check(self.status === 422, "un Admin no puede darse de baja a sí mismo", String(self.status));
    const rest = await call("POST", `/api/users/${created.user}/restore`, admin.token);
    check(rest.status === 200 && rest.data.active === true, "restaurar", String(rest.status));
    check((await call("POST", "/api/auth/login", null, { email, password: "Smoke1234" })).status === 200, "vuelve a poder entrar");
    const escal = await call("PUT", "/api/users/me", resident.token, { role: "Admin" });
    check(escal.status === 403, "un residente no puede cambiarse el rol", String(escal.status));
    await call("DELETE", `/api/users/${created.user}`, admin.token);
  }

  console.log("\n7) Flujo de portería: visita → permiso → validar → check-in → check-out");
  {
    const v = await call("POST", "/api/visits", resident.token, { nameVisit: "Visita", lastNameVisit: `Smoke${suffix}`, dniVisit: dni() });
    check(v.status === 201, "crear visitante", String(v.status)); created.visit = v.data?.visitId;
    const now = Date.now();
    const permit = await call("POST", "/api/entry-permissions", resident.token, {
      permissionType: "Visit", description: "smoke", validFrom: new Date(now - 3600e3).toISOString(), validTo: new Date(now + 7200e3).toISOString(),
      visitId: created.visit, userId: admin.id /* se ignora: un residente siempre crea a su nombre */,
    });
    check(permit.status === 201 && permit.data.resident.id === resident.id, "crear permiso (titular = residente, no el userId enviado)", String(permit.status)); created.permit = permit.data?.id;
    const code = permit.data?.qrCodeValue;
    const noWindow = await call("POST", "/api/entry-permissions", resident.token, { permissionType: "Visit", validFrom: new Date(now + 3600e3).toISOString(), validTo: new Date(now).toISOString(), visitId: created.visit, userId: resident.id });
    check(noWindow.status === 400, "ValidTo anterior a ValidFrom → 400", String(noWindow.status));
    const val = await call("GET", `/api/entry-permissions/validate?code=${code}`, security.token);
    check(val.status === 200 && val.data.checkResultStatus.startsWith("Autorizado"), "validar QR → autorizado", val.data?.checkResultStatus);
    check((await call("PATCH", `/api/entry-permissions/${created.permit}/check-in`, resident.token)).status === 403, "un residente no puede hacer check-in");
    const ci = await call("PATCH", `/api/entry-permissions/${created.permit}/check-in`, security.token);
    check(ci.status === 200 && ci.data.entryTime && ci.data.checkInGuard?.id === security.id, "check-in registra al guardia del token", String(ci.status));
    const ci2 = await call("PATCH", `/api/entry-permissions/${created.permit}/check-in`, security.token);
    check(ci2.status === 409, "segundo check-in → 409", String(ci2.status));
    const val2 = await call("GET", `/api/entry-permissions/validate?code=${code}`, security.token);
    check(val2.status === 200 && val2.data.checkResultStatus.startsWith("Dentro"), "validar de nuevo → dentro, marcar salida");
    const cancelUsed = await call("DELETE", `/api/entry-permissions/${created.permit}`, resident.token);
    check(cancelUsed.status === 409, "no se puede cancelar un permiso ya usado → 409", String(cancelUsed.status));
    const co = await call("PATCH", `/api/entry-permissions/${created.permit}/check-out`, security.token);
    check(co.status === 200 && co.data.departureTime, "check-out", String(co.status));
    const val3 = await call("GET", `/api/entry-permissions/validate?code=${code}`, security.token);
    check(val3.status === 409, "permiso consumido → 409", String(val3.status));
    const hist = await call("GET", `/api/entry-permissions/history?search=Smoke${suffix}`, security.token);
    check(hist.data?.totalCount === 1, "aparece en el historial", String(hist.data?.totalCount));

    // Permiso cancelado: el QR deja de validar
    const p2 = await call("POST", "/api/entry-permissions", resident.token, { permissionType: "Visit", validFrom: new Date(now - 3600e3).toISOString(), validTo: new Date(now + 7200e3).toISOString(), visitId: created.visit, userId: resident.id });
    await call("DELETE", `/api/entry-permissions/${p2.data.id}`, resident.token);
    const cancelledQr = await call("GET", `/api/entry-permissions/validate?code=${p2.data.qrCodeValue}`, security.token);
    check(cancelledQr.status === 409, "QR cancelado → 409 (el guardia ve que fue cancelado)", String(cancelledQr.status));
    // Permiso futuro: 422
    const p3 = await call("POST", "/api/entry-permissions", resident.token, { permissionType: "Visit", validFrom: new Date(now + 86400e3).toISOString(), validTo: new Date(now + 2 * 86400e3).toISOString(), visitId: created.visit, userId: resident.id });
    const early = await call("GET", `/api/entry-permissions/validate?code=${p3.data.qrCodeValue}`, security.token);
    check(early.status === 422, "permiso todavía no vigente → 422", String(early.status));
    await call("DELETE", `/api/entry-permissions/${p3.data.id}`, resident.token);
  }

  console.log("\n7b) Pase QR: formulario, token firmado, PDF, enlace público, escaneo y auditoría");
  {
    const now = Date.now();
    const win = { validFrom: new Date(now - 3600e3).toISOString(), validTo: new Date(now + 7200e3).toISOString() };
    const v = await call("POST", "/api/visits", resident.token, { nameVisit: "Pase", lastNameVisit: `Qr${suffix}`, dniVisit: dni() });
    const visitId = v.data?.visitId;

    const noPlate = await call("POST", "/api/invitations", resident.token, { permissionType: "Visit", ...win, visitId, userId: resident.id, vehicleType: "Car" });
    check(noPlate.status === 400 && /patente/i.test(noPlate.data?.message ?? ""), "auto sin patente → 400 con mensaje claro", String(noPlate.status));
    const tooMany = await call("POST", "/api/invitations", resident.token, { permissionType: "Visit", ...win, visitId, userId: resident.id, companionsCount: 99 });
    check(tooMany.status === 400, "demasiados acompañantes → 400", String(tooMany.status));

    const p = await call("POST", "/api/invitations", resident.token, {
      permissionType: "Visit", ...win, visitId, userId: resident.id,
      companionsCount: 2, vehicleType: "Car", vehiclePlate: "ab 123 cd", vehicleModel: "Fiat Cronos", passType: "SingleEntry",
    });
    check(p.status === 201 && p.data.vehiclePlate === "AB123CD" && p.data.companionsCount === 2 && p.data.passType === "SingleEntry",
      "POST /invitations crea el pase con acompañantes, vehículo y tipo de pase", String(p.status));
    const token = p.data?.qrCodeValue ?? "";
    check(/^[0-9a-f]{32}\.[A-Za-z0-9_-]{22}$/.test(token), "el QR contiene un token firmado (id + HMAC), no datos en claro");

    const pass = await call("GET", `/api/invitations/${p.data.id}/pass`, resident.token);
    check(pass.status === 200 && pass.data.token === token && pass.data.vehiclePlate === "AB123CD", "GET /invitations/{id}/pass", String(pass.status));
    check(!("email" in (pass.data ?? {})) && !("phone" in (pass.data ?? {})), "el pase no expone email ni teléfono del residente");

    const pdfRes = await fetch(`${API}/api/invitations/${p.data.id}/pass/pdf`, { headers: { Authorization: `Bearer ${resident.token}` } });
    const pdfBytes = new Uint8Array(await pdfRes.arrayBuffer());
    check(pdfRes.status === 200 && pdfRes.headers.get("content-type") === "application/pdf" && String.fromCharCode(...pdfBytes.slice(0, 5)) === "%PDF-" && pdfBytes.length > 2000,
      "PDF del pase generado", `${pdfRes.status}, ${pdfBytes.length} bytes`);

    const pub = await call("GET", `/api/passes/${token}`, null);
    check(pub.status === 200 && pub.data.visitorName === `Pase Qr${suffix}`, "enlace público del pase (sin sesión)", String(pub.status));
    const altered = token.slice(0, -1) + (token.endsWith("A") ? "B" : "A");
    check((await call("GET", `/api/passes/${altered}`, null)).status === 404, "token alterado en el enlace público → 404");

    check((await call("POST", "/api/invitations/scan", resident.token, { token })).status === 403, "un residente no puede escanear");
    check((await call("POST", "/api/invitations/scan", null, { token })).status === 401, "escanear sin sesión → 401");
    check((await call("POST", "/api/invitations/scan", security.token, {})).status === 400, "escanear sin token → 400");

    const forged = await call("POST", "/api/invitations/scan", security.token, { token: `${"0".repeat(32)}.AAAAAAAAAAAAAAAAAAAAAA` });
    check(forged.status === 200 && forged.data.success === false && forged.data.reason === "InvalidToken", "token falsificado → rechazado (InvalidToken)", forged.data?.reason);
    const unknown = await call("POST", "/api/invitations/scan", security.token, { token: "no-existe" });
    check(unknown.data?.success === false && unknown.data?.reason === "NotFound", "pase inexistente → rechazado (NotFound)", unknown.data?.reason);

    const ok = await call("POST", "/api/entry-permissions/scan", security.token, { token });
    check(ok.data?.success === true && ok.data.outcome === "CheckIn" && ok.data.pass?.visitorName === `Pase Qr${suffix}` && ok.data.pass.companionsCount === 2 && ok.data.pass.vehiclePlate === "AB123CD",
      "scan aprobado: check-in con la ficha completa", ok.data?.message);
    const used = await call("POST", "/api/entry-permissions/scan", security.token, { token });
    check(used.data?.success === false && used.data.reason === "AlreadyUsed", "segundo scan → Invitación ya utilizada", used.data?.reason);

    // Lista negra
    const v2 = await call("POST", "/api/visits", resident.token, { nameVisit: "Bloq", lastNameVisit: `Uea${suffix}`, dniVisit: dni() });
    const p2 = await call("POST", "/api/invitations", resident.token, { permissionType: "Visit", ...win, visitId: v2.data.visitId, userId: resident.id, passType: "MultipleEntries" });
    check((await call("PATCH", `/api/visits/${v2.data.visitId}/block`, resident.token, { blocked: true, reason: "x" })).status === 403, "un residente no puede bloquear visitantes");
    const blk = await call("PATCH", `/api/visits/${v2.data.visitId}/block`, security.token, { blocked: true, reason: "Smoke" });
    check(blk.status === 200 && blk.data.isBlocked === true, "Seguridad bloquea al visitante", String(blk.status));
    const blocked = await call("POST", "/api/invitations/scan", security.token, { token: p2.data.qrCodeValue });
    check(blocked.data?.success === false && blocked.data.reason === "Blocked", "visitante bloqueado → rechazado (Blocked)", blocked.data?.reason);
    await call("PATCH", `/api/visits/${v2.data.visitId}/block`, security.token, { blocked: false });

    // Múltiples ingresos
    const enter1 = await call("POST", "/api/invitations/scan", security.token, { token: p2.data.qrCodeValue });
    const inside = await call("POST", "/api/invitations/scan", security.token, { token: p2.data.qrCodeValue });
    const exit = await call("POST", "/api/invitations/scan", security.token, { token: p2.data.qrCodeValue, action: "CheckOut" });
    const enter2 = await call("POST", "/api/invitations/scan", security.token, { token: p2.data.qrCodeValue });
    check(enter1.data?.success && inside.data?.reason === "AlreadyInside" && exit.data?.outcome === "CheckOut" && enter2.data?.success,
      "pase de múltiples ingresos: entrar, no duplicar, salir y volver a entrar");

    // Fuera de horario
    const p3 = await call("POST", "/api/invitations", resident.token, { permissionType: "Visit", validFrom: new Date(now + 86400e3).toISOString(), validTo: new Date(now + 2 * 86400e3).toISOString(), visitId, userId: resident.id });
    const early = await call("POST", "/api/invitations/scan", security.token, { token: p3.data.qrCodeValue });
    check(early.data?.success === false && early.data.reason === "NotYetValid" && /Fuera de horario/.test(early.data.message), "fuera de horario → rechazado con motivo explícito", early.data?.reason);
    await call("DELETE", `/api/invitations/${p3.data.id}`, resident.token);

    // Cancelado
    const p4 = await call("POST", "/api/invitations", resident.token, { permissionType: "Visit", ...win, visitId, userId: resident.id });
    await call("DELETE", `/api/invitations/${p4.data.id}`, resident.token);
    const cancelled = await call("POST", "/api/invitations/scan", security.token, { token: p4.data.qrCodeValue });
    check(cancelled.data?.success === false && cancelled.data.reason === "Cancelled", "pase cancelado → rechazado (Cancelled)", cancelled.data?.reason);

    // Avisos al residente y sincronización de la guardia
    const sync = await call("GET", "/api/entry-permissions/sync", security.token);
    check(sync.status === 200 && Array.isArray(sync.data?.items) && sync.data.items.some((i) => i.token === p2.data.qrCodeValue && typeof i.blocked === "boolean"),
      "GET /entry-permissions/sync trae los pases activos con su token", String(sync.data?.items?.length));
    check((await call("GET", "/api/entry-permissions/sync", resident.token)).status === 403, "un residente no puede sincronizar");
    const unread = await call("GET", "/api/notifications/unread-count", resident.token);
    check(unread.status === 200 && unread.data.count >= 1, "el residente tiene avisos sin leer tras el ingreso", String(unread.data?.count));
    const notes = await call("GET", "/api/notifications?unreadOnly=true", resident.token);
    const note = notes.data?.items?.find((n) => n.type === "VisitCheckIn" && n.message.includes(`Pase Qr${suffix}`));
    check(!!note && note.message.endsWith("ha ingresado al barrio."), "aviso: Tu visita [nombre] ha ingresado al barrio", note?.message);
    check((await call("PATCH", `/api/notifications/${note?.id}/read`, security.token)).status === 404, "no se puede leer el aviso de otro usuario");
    check((await call("PATCH", `/api/notifications/${note?.id}/read`, resident.token)).status === 204, "marcar aviso como leído");
    check((await call("POST", "/api/notifications/read-all", resident.token)).status === 204, "marcar todos como leídos");
    check((await call("GET", "/api/notifications/unread-count", resident.token)).data?.count === 0, "sin avisos pendientes");

    // Auditoría
    const logs = await call("GET", `/api/access-logs?permissionId=${p.data.id}`, security.token);
    check(logs.status === 200 && logs.data.totalCount === 2 && logs.data.items.some((l) => l.outcome === "CheckIn" && l.guardId === security.id) && logs.data.items.some((l) => l.reason === "AlreadyUsed"),
      "auditoría: queda el ingreso y el intento fallido, con el guardia", String(logs.data?.totalCount));
    const rejected = await call("GET", "/api/access-logs?outcome=Rejected", security.token);
    check(rejected.data?.totalCount >= 5, "auditoría: también los tokens inválidos", String(rejected.data?.totalCount));
    check((await call("GET", "/api/access-logs", resident.token)).status === 403, "un residente no ve la auditoría");

    await call("DELETE", `/api/visits/${v2.data.visitId}`, admin.token);
    await call("DELETE", `/api/visits/${visitId}`, admin.token);
  }

  console.log("\n8) Reservas y solicitudes del residente");
  {
    const amen = await call("POST", "/api/amenities", admin.token, { amenityName: `Cancha ${suffix}`, description: "prueba", schedules: "9-22", capacity: 1 });
    const start = new Date(Date.now() + 5 * 86400e3); start.setUTCHours(15, 0, 0, 0);
    const end = new Date(start.getTime() + 2 * 3600e3);
    const t1 = await call("POST", "/api/turns", resident.token, { amenityId: amen.data.id, startTime: start.toISOString(), endTime: end.toISOString() });
    check(t1.status === 201 && t1.data.turnStatus === "Pending", "crear reserva", String(t1.status)); created.turn = t1.data?.id;
    const t2 = await call("POST", "/api/turns", admin.token, { amenityId: amen.data.id, startTime: new Date(start.getTime() + 3600e3).toISOString(), endTime: new Date(end.getTime() + 3600e3).toISOString() });
    check(t2.status === 409, "solapamiento con capacidad 1 → 409", String(t2.status));
    const t3 = await call("POST", "/api/turns", resident.token, { amenityId: amen.data.id, startTime: end.toISOString(), endTime: start.toISOString() });
    check(t3.status === 400, "Start > End → 400", String(t3.status));
    check((await call("GET", `/api/turns/${created.turn}`, security.token)).status === 403, "Seguridad no ve reservas");
    const mine = await call("GET", "/api/turns/me", resident.token);
    check(mine.data.items.some((t) => t.id === created.turn), "aparece en /turns/me");
    const cancel = await call("DELETE", `/api/turns/${created.turn}`, resident.token);
    check(cancel.status === 200 && cancel.data.turnStatus === "Cancelled", "cancelar reserva", String(cancel.status));
    const t4 = await call("POST", "/api/turns", admin.token, { amenityId: amen.data.id, startTime: start.toISOString(), endTime: end.toISOString() });
    check(t4.status === 201, "el horario cancelado vuelve a estar libre", String(t4.status));
    await call("DELETE", `/api/turns/${t4.data?.id}`, admin.token);
    await call("DELETE", `/api/amenities/${amen.data.id}`, admin.token);

    const rq = await call("POST", "/api/requests", resident.token, { orderId: created.order, details: "Pérdida de agua en la cocina", location: "Lote Demo", userId: admin.id });
    check(rq.status === 201 && rq.data.user.id === resident.id, "crear solicitud (solicitante = token)", String(rq.status)); created.request = rq.data?.id;
    check((await call("GET", `/api/requests/${created.request}`, resident.token)).status === 200, "el residente ve su solicitud");
    const rqDel = await call("DELETE", `/api/requests/${created.request}`, resident.token);
    check(rqDel.status === 200 && rqDel.data.status === "Cancelled", "cancelar solicitud", String(rqDel.status));
  }

  console.log("\n9) Limpieza");
  for (const [path, id] of [["orders", created.order], ["lots", created.lot], ["visits", created.visit]])
    if (id) await call("DELETE", `/api/${path}/${id}`, admin.token);

  console.log(failures ? `\n✗ ${failures} verificación(es) fallaron` : "\n✓ Todo OK");
  process.exit(failures ? 1 : 0);
}

main().catch((e) => { console.error("Error en la prueba:", e); process.exit(2); });
