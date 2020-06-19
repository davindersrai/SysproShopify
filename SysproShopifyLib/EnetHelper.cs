using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SYSPROWCFServicesClientLibrary40;
using System.Xml;
using SysproShopifyLib.Model;

namespace HCMAShipPrimusSysLib.Helpers
{
    public class EnetHelper
    {
        private SYSPROWCFServicesPrimitiveClient _client;
        private string _sysproGuid;
        private string _sysproOperator;
        private string _sysproOperatorPwd;
        private string _sysproCompany;
        private string _sysproCompanyPwd;
        private string _wcfBaseAddress;
        private const string _businessObject = "";
        private string _xmlParams = String.Empty;
        private string _xmlInput = String.Empty;
        private string _xmlOutput = String.Empty;

        public EnetHelper(string baseAddress, string sysproOperator, string sysproOperatorPwd, string sysproCompany, string sysproCompanyPwd)
        {
            _wcfBaseAddress = baseAddress;
            _sysproOperator = sysproOperator;
            _sysproOperatorPwd = sysproOperatorPwd;
            _sysproCompany = sysproCompany;
            _sysproCompanyPwd = sysproCompanyPwd;
            _sysproGuid = String.Empty;
        }

        /*
         * GetCustomerPrice
         * Using the Sales Order Line Build this will return Current Price and Price Based on SalsOrder Qty
         */
        public CustomerPriceModel GetCustomerPrice(string customer, string stockCode, decimal quantity)
        {
     

            try
            {
                LogonToSYSPRO();
                StringBuilder sXmlIn = new StringBuilder();
                sXmlIn.Append("<Build>");
                sXmlIn.Append("<Parameters> ");
                sXmlIn.Append("<StockCode>A100</StockCode> ");
                sXmlIn.Append("<Customer>10</Customer> ");
                sXmlIn.Append("<OrderQuantity>23.1</OrderQuantity> ");
                sXmlIn.Append("<OrderUm>EA</OrderUm> ");
                sXmlIn.Append("</Parameters> ");
                sXmlIn.Append("</Build>");
                _xmlOutput = _client.TransactionBuild(_sysproGuid, "SORRSL", sXmlIn.ToString());
                CustomerPriceModel getCustomerPriceModel = new CustomerPriceModel();
                XmlDocument doc= GetSysproOutputDoc(_xmlOutput);
                XmlNode currentPrice = doc.SelectSingleNode("//CurrentPrice");
                XmlNode priceBasedOnQty = doc.SelectSingleNode("//PriceBasedOnQty");

                if (currentPrice !=null)
                {
                    getCustomerPriceModel.CurrentPrice = decimal.Parse(currentPrice.InnerText);
                }
                if (priceBasedOnQty != null)
                {
                    getCustomerPriceModel.PriceBasedOnQty = decimal.Parse(priceBasedOnQty.InnerText);
                }
                LogoffOfSYSPRO();
                return getCustomerPriceModel;

            }
            catch (Exception ex)
            {
                if (_client != null) LogoffOfSYSPRO();
                throw ex;
            }
        }
        /*
         * PostFreightLines
         * Post the freight line to the specified sales order with the SYSPRO WCF library 
         */
        public void CreateSalesOrder(SalesOrderModel salesOrderModel)
        {           
            try
            {
                LogonToSYSPRO();
                StringBuilder sXmlIn = new StringBuilder();
                sXmlIn.Append("<SalesOrders><Orders>");
                sXmlIn.Append("<OrderHeader>");
                sXmlIn.Append("<Customer>" + salesOrderModel.Customer + "</Customer>");
                sXmlIn.Append("<CustomerPoNumber>" + salesOrderModel.CustomerPoNumber + "</CustomerPoNumber>");
                sXmlIn.Append("</OrderHeader>");
                sXmlIn.Append("<OrderDetails>");

                foreach (SalesOrderLine line in salesOrderModel.lines)
                {
                    sXmlIn.Append("<StockLine>");
                    sXmlIn.Append("<CustomerPoLine>" + line.CustomerPoLine + "</CustomerPoLine>");
                    sXmlIn.Append("<StockCode>" + line.StockCode + "</StockCode>");
                    sXmlIn.Append("<OrderQty>" + line.OrderQty + "</OrderQty>");
                    sXmlIn.Append("<OrderUom>EA</OrderUom>");
                    sXmlIn.Append("</StockLine>");

                }
                sXmlIn.Append("</OrderDetails>");
                sXmlIn.Append("</Orders></SalesOrders>");
                _xmlOutput = _client.TransactionPost(_sysproGuid, "SORTOI", CreateXMLParams(), sXmlIn.ToString());
                XmlDocument doc = GetSysproOutputDoc(_xmlOutput);
                SalesOrderPostModel postModel = new SalesOrderPostModel();

                XmlNode salesOrderNode = doc.SelectSingleNode("//SalesOrder");
                if (salesOrderNode != null)
                {
                    postModel.SalesOrderNumber = salesOrderNode.InnerText;
                }
                LogoffOfSYSPRO();                

            }
            catch (Exception ex)
            {
                if (_client != null) LogoffOfSYSPRO();
                throw ex;
            }
        }
        private string CreateXMLParams()
        {
            StringBuilder parameter = new StringBuilder();
            parameter.Append("<SalesOrders>");
            parameter.Append("<Parameters>");
            parameter.Append("<Process>IMPORT</Process>");
            parameter.Append("</Parameters>");
            parameter.Append("</SalesOrders>");
            return parameter.ToString();
        }

        /*
         * CreateXMLBody
         * Creates the body for the business obejct interaction
         */

        /*
         * HandleTransactionOutput
         * Ensure the xmlOutput is non empty/null, then parse it for Error or Exception nodes and throw an exception containning the error information
         */
        private void HandleTransactionOutput(string xmlOutput)
        {
            string errorMessages = String.Empty;

            if (!String.IsNullOrEmpty(xmlOutput))
            {
                var doc = new XmlDocument();

                try
                {
                    doc.LoadXml(xmlOutput);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not parse e.NET response: {ex.Message}");
                }

                if (xmlOutput.IndexOf("Error") > 0 || xmlOutput.IndexOf("Exception") > 0)
                {

                    if (xmlOutput.IndexOf("Error") > 0)
                    {
                        if (doc.SelectSingleNode("//ErrorDescription") != null)
                        {
                            foreach (XmlNode n in doc.SelectNodes("//ErrorDescription"))
                            {
                                errorMessages += n.InnerXml + Environment.NewLine;
                            }
                        }
                    }

                    if (xmlOutput.IndexOf("Exception") > 0)
                    {
                        if (doc.SelectSingleNode("//Message") != null)
                        {
                            foreach (XmlNode n in doc.SelectNodes("//Message"))
                            {
                                errorMessages += n.InnerXml + Environment.NewLine;
                            }
                        }
                    }

                    throw new Exception($"Error while posting freight to SYSPRO: {errorMessages}");
                }
                else
                {
                    return;
                }
            }
            else
            {
                throw new Exception("Error posting freight to SYSPRO: Empty e.NET response. ");
            }
        }
        private XmlDocument GetSysproOutputDoc(string xmlOutput)
        {

            if (!String.IsNullOrEmpty(xmlOutput))
            {
                var doc = new XmlDocument();

                try
                {
                    doc.LoadXml(xmlOutput);
                    return doc;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not parse e.NET response: {ex.Message}");
                }
                
            }
            return null;
        }


        /*
         * LogonToSYSPRO
         * Log the client binding on to the WCF service
         */
        private void LogonToSYSPRO()
        {
            if (_client == null)
            {
                InitializSysproWcfClient();
            }

            try
            {
                if (_sysproGuid == String.Empty)
                {
                    _sysproGuid = _client.Logon(_sysproOperator, _sysproOperatorPwd, _sysproCompany, _sysproCompanyPwd);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occured during SYSPRO logon: {ex.Message}");
            }
        }

        /*
         * LogoffSYSPRO
         * Log the client binding off of the WCF service
         */
        private void LogoffOfSYSPRO()
        {
            if (_client != null)
            {
                try
                {
                    if (_sysproGuid != String.Empty)
                    {
                        _client.Logoff(_sysproGuid);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            _sysproGuid = String.Empty;
        }

        /*
         * InitializSysproWcfClient
         * Create the syspro client with a NetTcp binding
         */
        private void InitializSysproWcfClient()
        {
            try
            {                
                _client = new SYSPROWCFServicesPrimitiveClient(_wcfBaseAddress, SYSPROWCFBinding.NetTcp);
            }
            catch (Exception ex)
            {
                throw new Exception( "WCF Initialization Error: " + ex.Message);
            }
        }
    }
}
