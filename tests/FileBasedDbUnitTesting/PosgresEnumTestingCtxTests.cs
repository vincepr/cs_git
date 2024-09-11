namespace FileBasedDbUnitTesting;

public class PosgresEnumTestingCtxTests
{
    [Test]
    public async Task Run()
    {
        var c = Postgresqlconn.Create();
        var l = c.Models.ToList();
        foreach (var customer in l)
        {
            Console.WriteLine(customer.Id);
            Console.WriteLine(customer.Name);
            Console.WriteLine(customer.Material);
        }


    }
}