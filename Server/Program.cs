using Server.Database;

using (DBC db = new ()) 
{
    var users = db.customers.ToList();
    Console.WriteLine("All users in database:");
    foreach(var user in users) 
    {
        Console.WriteLine($"{user.id} {user.name} - Пароль:{user.password}, Категория:{user.description}");
    }
}

using (DBC db = new()) 
{ 

}