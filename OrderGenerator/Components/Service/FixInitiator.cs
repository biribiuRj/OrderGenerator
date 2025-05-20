using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.Components.Service
{
    public class FixInitiator : MessageCracker, IApplication
    {
        private readonly SocketInitiator _initiator;
        private SessionID? _sessionID;

        public FixInitiator()
        {
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string configPath = Path.Combine(projectRoot, "config.cfg");

            var settings = new SessionSettings(configPath);
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);
            var application = this;

            _initiator = new SocketInitiator(application, storeFactory, settings, logFactory);
        }

        public void StartSession()
        {
            _initiator.Start();

            Console.WriteLine("FIX Session iniciada.");
        }

        public void StopSession()
        {
            _initiator.Stop();
            Console.WriteLine("FIX Session Parada.");
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }
        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID)
        {
            _sessionID = sessionID;
        }
        public SessionID GetSessionID() => _sessionID;
        public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
        public void ToApp(QuickFix.Message message, SessionID sessionID) { }
        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            Console.WriteLine(message);
        }

        public string SendNewOrder(string symbol, string side, int quantity, decimal price)
        {
            try
            {
                var order = new NewOrderSingle(
                    new ClOrdID(Guid.NewGuid().ToString()),
                    new Symbol(symbol),
                    new Side(side == "BUY" ? Side.BUY : Side.SELL),
                    new TransactTime(DateTime.UtcNow),
                    new OrdType(OrdType.MARKET)
                );

                order.Set(new Price(price));
                order.Set(new OrderQty(quantity));

                Session.SendToTarget(order, _sessionID);
                return true ? "Ordem enviada com sucesso!" : "Falha ao enviar a ordem.";
            }
            catch (Exception e)
            {
                Console.WriteLine("Error na criação da Ordem:", e.Message);
                return false ? "Ordem enviada com sucesso!" : "Falha ao enviar a ordem.";
            }
        }
    }
}