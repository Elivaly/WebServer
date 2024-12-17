using Server.Database;

using (DBC db = new DBC()) 
{
    customers customer1 = new customers { name = "Luis", password = "French", description = "Client"};
    customers customer2 = new customers { name = "Anya", password = "bert1234", description = "Guest" };

    db.customers.AddRange(customer1, customer2);
    db.SaveChanges();
}
using (DBC db = new DBC()) 
{
    var users = db.customers.ToList();
    Console.WriteLine("All users in database:");
    foreach(var user in users) 
    {
        Console.WriteLine($"{user.id} {user.name} - Пароль:{user.password}, Категория:{user.description}");
    }
}