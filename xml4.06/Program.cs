using System;
using System.Xml;
using static System.Console;

namespace HomeworkLesson11
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "Orders.xml";

            // Метод для создания XML
            CreateOrdersXml(fileName);
            WriteLine($"Файл '{fileName}' создан.\n");

            // Метод для чтения через XmlDocument
            ReadWithXmlDocument(fileName);
            WriteLine();

            // Метод для чтения через XmlTextReader
            ReadWithXmlTextReader(fileName);

            WriteLine("\nГотово! Нажмите Enter для выхода...");
            ReadLine();
        }

        // Метод для создания XML
        static void CreateOrdersXml(string filename)
        {
            using (XmlTextWriter writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Orders");

                // Первый заказ
                writer.WriteStartElement("Order");
                writer.WriteAttributeString("Id", "1");
                writer.WriteAttributeString("Date", "2023-10-27");
                writer.WriteAttributeString("Customer", "Иван Петров");

                writer.WriteStartElement("Item");
                writer.WriteAttributeString("Product", "Ноутбук");
                writer.WriteAttributeString("Price", "50000");
                writer.WriteAttributeString("Quantity", "1");
                writer.WriteEndElement();

                writer.WriteStartElement("Item");
                writer.WriteAttributeString("Product", "Мышь");
                writer.WriteAttributeString("Price", "1500");
                writer.WriteAttributeString("Quantity", "2");
                writer.WriteEndElement();

                writer.WriteEndElement();

                // Второй заказ
                writer.WriteStartElement("Order");
                writer.WriteAttributeString("Id", "2");
                writer.WriteAttributeString("Date", "2023-10-28");
                writer.WriteAttributeString("Customer", "Мария Смирнова");

                writer.WriteStartElement("Item");
                writer.WriteAttributeString("Product", "Книга");
                writer.WriteAttributeString("Price", "800");
                writer.WriteAttributeString("Quantity", "3");
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        // Метод для чтения через XmlDocument
        static void ReadWithXmlDocument(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            // Выбираем все заказы
            XmlNodeList orders = doc.SelectNodes("/Orders/Order");

            foreach (XmlNode order in orders)
            {
                WriteLine($"Заказ #{order.Attributes["Id"]?.Value} | Дата: {order.Attributes["Date"]?.Value} | Клиент: {order.Attributes["Customer"]?.Value}");
                
                // Перебираем товары внутри заказа
                foreach (XmlNode item in order.ChildNodes)
                {
                    if (item.Name == "Item")
                    {
                        WriteLine($"   - Товар: {item.Attributes["Product"]?.Value}, Цена: {item.Attributes["Price"]?.Value}, Количество: {item.Attributes["Quantity"]?.Value}");
                    }
                }
                WriteLine();
            }
        }

        // Метод для чтения через XmlTextReader
        static void ReadWithXmlTextReader(string filename)
        {
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    // Если встретили открывающий тег заказа
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Order")
                    {
                        string currentOrderId = reader.GetAttribute("Id") ?? "?";
                        string currentCustomer = reader.GetAttribute("Customer") ?? "?";
                        WriteLine($"Заказ #{currentOrderId} | Клиент: {currentCustomer}");
                    }

                    // Если встретили открывающий тег товара
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Item")
                    {
                        string product = reader.GetAttribute("Product") ?? "?";
                        string price = reader.GetAttribute("Price") ?? "?";
                        string quantity = reader.GetAttribute("Quantity") ?? "?";
                        WriteLine($"   - Товар: {product}, Цена: {price}, Количество: {quantity}");
                    }
                }
            }
        }
    }
}