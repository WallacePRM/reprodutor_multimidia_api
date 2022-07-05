namespace ReprodutorMultimidia.Models;

public class ResponseError
{
    public string Message { get; set; }

    public ResponseError(string message)
    {
        this.Message = message;
    }
}