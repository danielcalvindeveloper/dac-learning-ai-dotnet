public interface IAssistant
{
    Task<Persona> ExtraerPersonaAsync(string texto);

    Task<Personas> ExtraerPersonasAsync(string texto);

    Task<Producto> ExtraerProductoAsync(string texto);
}
