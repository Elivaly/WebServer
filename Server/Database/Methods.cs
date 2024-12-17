using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Database
{
    public class Methods
    {
        public void getCutomers() 
        {
            using(DBC db = new()) 
            {
                var customers = db.customers.ToList();
                foreach (var customer in customers) 
                {
                    Console.WriteLine(customer);
                }
            }
        }

        public void addCustomer() 
        {
        
        }

        public void deleteCustomer() 
        {
        
        }

        public void modifyCustomer() 
        {
        
        }
    }
}
