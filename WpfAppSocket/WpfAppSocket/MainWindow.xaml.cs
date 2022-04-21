using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets; //aggiunta
using System.Net; //aggiunta
using System.Windows.Threading; //aggiunta


/*SCAMBIO DI DATI A BASSO LIVELLO, VERSIONE PEER TO PEER (NO CLIENT SERVER), INDIRIZZO IP DEL DESTINATARIO, PORTA E MESSAGGIO, QUANDO RICEVO UN MESSAGGIO COMPARE NELLA LISTBOX
 PARTE MITTENTE CONTIENE UN TIMER PER VEDERE SE SI RICEVONO MESSAGGI PER EVITARE DI USARE THREAD
 */
namespace WPF_Socket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket = null; //creazione oggetto socket, impostato a null
        DispatcherTimer dTimer = null;

        public MainWindow()
        {
            InitializeComponent();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.EnableBroadcast = true; //abilitiamo broadcast
            //address family è un enumerativo che permette di specificare le modalità di lavoro (lavoreremo con indirizzi ipv4)
            //socket type è un enumerativo che permette di usare il protocollo UDP
            //protocol type permette di specificare il protocollo, noi usiamo UDP (senza connessione)

            //dobbiamo mettere in comunicazione il nostro pc con quello del destinatario, quindi avremo bisogno di 1 socket sorgente ed 1 destinatario
            IPAddress local_address = IPAddress.Any;
            IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), 65400); //il socket ha bisogno di 2 endpoint, 1 mittente e 1 destinatario, stiamo creando l'endpoint mittente, local address.maptoipv4 è l'indirizzo della nostra macchina, usiamo la porta 65000 perchè è una delle porte sbloccate(non bloccate dal firewall)

            socket.Bind(local_endpoint); //metodo che unisce il socket al local end point

            dTimer = new DispatcherTimer();

            dTimer.Tick += new EventHandler(aggiornamento_dTimer);  //aggiungo quale evento eseguire ogni tot tempo, si interrompe quello che si stava facendo per eseguire l'evento
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250); //definisco l'intervallo di tempo del timer, ogni 250milli secondi vado ad eseguire l'evento 
            dTimer.Start();
        }

        private void aggiornamento_dTimer(object sender, EventArgs e)
        {
            int nBytes = 0; //dichiaro variabile per contare i byte ricevuti

            if ((nBytes = socket.Available) > 0) //se ho dei byte disponibili da leggere
            {
                //ricezione dei caratteri in attesa
                byte[] buffer = new byte[nBytes]; //vettore di byte

                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); //metto ip any perchè in questo momento non so chi è

                nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint); //comando per ricevere un datagramma e memorizzare l'ip di chi me l'ha mandato

                string from = ((IPEndPoint)remoteEndPoint).Address.ToString(); //memorizzo ip del mittente in formato stringa

                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes); //trasformo in stringa i byte ricevuti, qual'e il buffer, indice del buffer(salviamo dall'inizio) e quanti byte caricare

                lstBox.Items.Add(from + ": " + messaggio);
            }
        }

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            IPAddress remote_address = IPAddress.Parse(txtIP.Text); //creazione ip del destinatario
            IPEndPoint remote_endpoint = new IPEndPoint(remote_address, int.Parse(txtPorta.Text)); //creazione end point del ricevente 
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);  //creo vettore di byte che conterrà il messaggio che verrà codificato in UTF8
            socket.SendTo(messaggio, remote_endpoint); //metodo che invia il messaggio dal socket

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IPAddress remote_address2 = IPAddress.Parse("255.255.255.255.255"); //USO QUESTO INDIRIZZO DI BROADCAST PER POTER COMUNICARE CON TUTTA LA SUBNET LOCALE
            IPEndPoint remote_endpoint2 = new IPEndPoint(remote_address2, int.Parse(txtPorta2.Text));  
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio2.Text);  //creo vettore di byte che conterrà il messaggio che verrà codificato in UTF8
            socket.SendTo(messaggio, remote_endpoint2);
        }
    }
}