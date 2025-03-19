using System.Data;
using System.Runtime.Serialization;

public class Kasyanov_NRIDataSet : DataSet
{
    public Kasyanov_NRIDataSet()
    {
        // Инициализация вашего DataSet
    }

    // Конструктор для сериализации (если нужен)
    protected Kasyanov_NRIDataSet(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}