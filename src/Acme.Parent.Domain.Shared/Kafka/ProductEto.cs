using Volo.Abp.EventBus;

namespace Acme.Parent.Kafka
{
    [EventName("Acme.Product")]
    public class ProductEto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
