using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Tests;

/// <summary>La baja lógica se expresa solo con DeletedAt y el filtro global oculta lo dado de baja.</summary>
public class SoftDeleteTests
{
    [Fact]
    public async Task Una_entidad_dada_de_baja_desaparece_de_las_consultas_y_se_puede_restaurar()
    {
        using var db = TestSupport.NewDb();
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        amenity.SoftDelete("tester");
        await db.SaveChangesAsync();

        Assert.False(await db.Amenities.AnyAsync(a => a.Id == amenity.Id));
        Assert.True(await db.Amenities.IgnoreQueryFilters().AnyAsync(a => a.Id == amenity.Id));

        amenity.Restore("tester");
        await db.SaveChangesAsync();

        Assert.True(await db.Amenities.AnyAsync(a => a.Id == amenity.Id));
    }

    [Fact]
    public async Task SoftDelete_es_idempotente_y_conserva_la_fecha_original()
    {
        using var db = TestSupport.NewDb();
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        amenity.SoftDelete();
        var firstDate = amenity.DeletedAt;
        amenity.SoftDelete();

        Assert.Equal(firstDate, amenity.DeletedAt);
        Assert.True(amenity.IsDeleted);
    }
}
