using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace ExploreCalifornia.EmailService
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // We need to declare a queue and a binding
            // You can declare a queue by calling the channel.QueueDeclare method.
            // The first parameter is our queue name. I'll call mine emailServiceQueue.
            // The second parameter is whether we want to make a durable queue or not.A durable queue will survive a broker restart. So let's pass in true here.
            // Next is a parameter indicating whether we want the queue to be exclusive to this connection or not.We could imagine having multiple instances of our email service running so we'll pass in false.
            // Finally, I don't want my queue to automatically be deleted when I disconnect. So I'll pass in false for the last argument as well.
            channel.QueueDeclare("emailServiceQueue", true, false, false);

            //  Now we need to bind this queue to the exchange.
            //  For this we'll use the channel.QueueBind method.
            //  The first argument is our queue name, emailServiceQueue in our example.
            //  And the second argument is the exchange name, which was webappExchange.
            //  Finally there's also a routing key but because we're using a fanout exchange which ignores the routing key we can just pass in an empty string here.
            //  We now have a queue and it's bound to the exchange but we're not consuming it yet.
            channel.QueueBind("emailServiceQueue", "webappExchange", "");


            //  We need a consumer for this.
            //  We'll construct a new instance of the EventingBasicConsumer which we can find in the RabbitMQ.Clients.Events namespace. So we'll add a using statement first, using RabbitMQ.Client.Events,
            //  and then we can construct a new instance of the EventingBasicConsumer. This constructor requires us to pass in the channel we created earlier.
            //  The EventingBasicConsumer will fire a Received event whenever it receives a message.
            //  So let's add some code to handle this event. We'll write a message to the console but as AMQP is a binary protocol we need to decode the raw bytes first.
            //  So we call the System.Text.Encoding .UTF8.GetString method. And we can find the raw bytes in the eventArgs.Body property. Then we can call Console.WriteLine and pass in our message.
            //  As a last step tell the channel to start consuming with the BasicConsume method. We'll pass in emailServiceQueue as queue name and our consumer, and also the second parameter is a Boolean indicating if we want to automatically acknowledge the fact that we received the message.
            //  This is important so RabbitMQ knows that it can safely delete the message from the queue.
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(eventArgs.Body.Span);
                Console.WriteLine(msg);
            };

            channel.BasicConsume("emailServiceQueue", true, consumer);
            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
