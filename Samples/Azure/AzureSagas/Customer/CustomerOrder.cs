using System;
using System.Windows.Forms;
using CashierContracts;
using CustomerContracts;
using NServiceBus;

namespace Customer
{
    public partial class CustomerOrder : Form,
                                         IHandleMessages<PaymentRequestMessage>,
                                         IHandleMessages<OrderReadyMessage>
    {
        private readonly IBus _bus;

        public CustomerOrder(IBus bus)
        {
            _bus = bus;
            InitializeComponent();
        }

        private void OrderButton_Click(Object sender, EventArgs e)
        {
            var customerName = NameTextBox.Text;
            var drink = DrinkComboBox.Text;
            var drinkSize = (DrinkSize) Enum.Parse(typeof(DrinkSize), SizeComboBox.Text);

            var newOrder = _bus.CreateInstance<NewOrderMessage>();
            newOrder.CustomerName = customerName;
            newOrder.Drink = drink;
            newOrder.DrinkSize = drinkSize;
            _bus.Send(newOrder);
        }

        public new void Handle(PaymentRequestMessage message)
        {
            var text = String.Format("Hi {0}, thanks for your order. That'll be �{1} please.", message.CustomerName, message.Amount);
            var result = MessageBox.Show(text, "Request payment", MessageBoxButtons.YesNo);

            var amount = (DialogResult.Yes == result) ? message.Amount : 0.0;
            var payment = _bus.CreateInstance<PaymentMessage>();
            payment.Amount = amount;
            payment.OrderId = message.OrderId;

            _bus.Reply(payment);    
        }

        public new void Handle(OrderReadyMessage message)
        {
            var text = String.Format("Here you go {0}, enjoy your {1}. Thank you, come again.", message.CustomerName, message.Drink);
            MessageBox.Show(text);
        }
    }
}