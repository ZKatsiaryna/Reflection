using System;
using System.Reflection;
using MyIoC;

namespace ConsoleAppRef
{
    class Program
    {

        static void Main(string[] args)
        {
            var container = new Container();
            //container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
            //var customerDAL = container.CreateInstance<ICustomerDAL>();
            //container.AddType(typeof(CustomerBLL));

            //container.AddType(typeof(Logger));
            //var ins = container.CreateInstance<CustomerBLL>();

            container = new Container();
            Type t = typeof(CustomerBLL2);
            container.AddAssembly(t.Assembly);
            var customerBLL = container.CreateInstance<CustomerBLL>();
            var customerBLL2 = container.CreateInstance<CustomerBLL2>();
            customerBLL2.Logger.ToString();
            Console.ReadLine();


        }
    }
}
