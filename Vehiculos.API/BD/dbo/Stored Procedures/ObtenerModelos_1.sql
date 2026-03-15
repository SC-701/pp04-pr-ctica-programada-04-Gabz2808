create PROCEDURE ObtenerModelos
    @IdMarca UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Modelos.[Id],
        Modelos.[IdMarca],
        Modelos.[Nombre]
    FROM [Vehiculos].[dbo].[Modelos] Modelos
    INNER JOIN Marcas 
        ON Marcas.Id = Modelos.IdMarca
    WHERE Modelos.IdMarca = @IdMarca
END