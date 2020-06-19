using HCMAShipPrimusSysLib.Helpers;
using SysproShopifyLib.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            EnetHelper enetService;

            string baseAddress = System.Configuration.ConfigurationManager.AppSettings["wcfBaseAddress"].ToString();
            string sysOperator = System.Configuration.ConfigurationManager.AppSettings["sysproOperator"].ToString();
            string operatorPwd = System.Configuration.ConfigurationManager.AppSettings["sysproOperatorPwd"].ToString();
            string company = System.Configuration.ConfigurationManager.AppSettings["sysproCompany"].ToString();
            string companyPwd = System.Configuration.ConfigurationManager.AppSettings["sysproCompanyPwd"].ToString();
            enetService = new EnetHelper(baseAddress, sysOperator, operatorPwd, company, companyPwd);

            enetService.GetCustomerPrice("0000001", "ABC", 212);

            SalesOrderModel salesOrder = new SalesOrderModel();
            List<SalesOrderLine> salesOrderLines = new List<SalesOrderLine>();
            salesOrder.Customer = "0000001";
            salesOrder.CustomerPoNumber = "Test14"; 
            SalesOrderLine line1 = new SalesOrderLine();
            line1.StockCode = "A100";
            line1.OrderQty = 323;
            line1.CustomerPoLine = 1;

            salesOrderLines.Add(line1);
            salesOrder.lines = salesOrderLines;
            enetService.CreateSalesOrder(salesOrder);


        }
    }

}
