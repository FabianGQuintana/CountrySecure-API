using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IOrderRepository: IGenericRepository<Order>
{
    // Obtener todos los servicios
    Task<IEnumerable<Order>> GetAllAsync();


    // Obtener servicios por estado (activo/inactivo)
    Task<IEnumerable<Order>> GetByStatusAsync(bool activo);

    // Obtener servicios por tipo (enum)
    Task<IEnumerable<Order>> GetByTypeAsync(OrderStatus tipoServicio);

    // Obtener servicios por proveedor
    Task<IEnumerable<Order>> GetBySupplierAsync(string nombreProveedor);
    // Obtener los servicios más solicitados
    Task<IEnumerable<Order>> GetMostRequestedAsync();

}
