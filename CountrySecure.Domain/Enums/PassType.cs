namespace CountrySecure.Domain.Enums
{
    public enum PassType
    {
        /// <summary>Un solo ingreso: después del check-in el pase queda utilizado.</summary>
        SingleEntry = 1,

        /// <summary>Varios ingresos y salidas dentro de la ventana horaria autorizada.</summary>
        MultipleEntries = 2
    }
}
