using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BasicDriverNET
{
public class RaceConnector {

	private String serverName;
	private int port;
	private IDriver driver;
	private bool stop = false;

    public String ReadLine()
    {
        int ch = inStream.Read();
        if (ch == -1)
        {
            return null;
        }
        StringBuilder sb = new StringBuilder();
        while (ch != '\n' && ch != -1)
        {
            sb.Append((char)ch);
            ch = inStream.Read();
        }
        return sb.ToString();
    }

    public void Connect()
    {
        socket = new Socket(SocketType.Stream, ProtocolType.IP);
        socket.Connect(serverName, port);
        inStream = new StreamReader(new NetworkStream(socket), Encoding.UTF8, false);
        outStream = new StreamWriter(new NetworkStream(socket), new UTF8Encoding(false));

    }
	public RaceConnector()
    {
	}
	public RaceConnector(String serverName, int port, IDriver driver) {
		this.serverName = serverName;
		this.port = port;
		this.driver = driver;
	}
	public String getServerName() {
		return serverName;
	}
	public void setServerName(String serverName) {
		this.serverName = serverName;
	}
	public int getPort() {
		return port;
	}
	public void setPort(int port) {
		this.port = port;
	}
	public IDriver getDriver() {
		return driver;
	}
	public void setDriver(IDriver driver) {
		this.driver = driver;
	}

	public void Stop(){
		stop = true;
		try {
			socket.Close();
		} catch (IOException e) {}
	}
	private Socket socket; // spojeni
	private StreamReader  inStream; // cteni se serveru
	private StreamWriter outStream; // zapis na server

	/**
	 * Pripoji se k zavodu.
	 * 
	 * @param host zavodni server
	 * @param port port serveru
	 * @param raceName nazev zavodu, do nehoz se chce klient pripojit
	 * @param driverName jmeno ridice
	 * @throws java.lang.IOException  problem s pripojenim
	 */
	public void start(String raceName, String driverName, String carType) {
		// pripojeni k serveru
        Connect();			// pripojeni k zavodu
        //outStream = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), "UTF-8"));
        //inStream = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));

		// pripojeni k zavodu
		outStream.Write("driver\n");                     // specifikace protokolu
		outStream.Write("race:" + raceName + "\n");      // nazev zavodu
		outStream.Write("driver:" + driverName + "\n");  // jmeno ridice
		outStream.Write("color:0000FF\n");               // barva auta
		if(carType != null){
			outStream.Write("car:" + carType + "\n");  // jmeno ridice
		}
		outStream.Write("\n");
		outStream.Flush();

		// precteni a kontrola dopovedi serveru
		String line = ReadLine();
		if (!line.Equals("ok")) {
			// pokud se pripojeni nepodari, je oznamena chyba a vyvolana vyjimka
			Console.WriteLine("Chyba: " + line);
			throw new ApplicationException(line);
		}
		ReadLine();  // precteni prazdneho radku
		run();
	}

	public List<String> listRaces() {
		List<String> raceList = new List<String>();
		Socket socket = null;
		try{
            Connect();			// pripojeni k zavodu
			outStream.Write("racelist\n");                     // specifikace protokolu
			outStream.Write("\n");
			outStream.Flush();

			// precteni a kontrola dopovedi serveru
			String line = ReadLine();
			if (!line.Equals("ok")) {
				// pokud se pripojeni nepodari, je oznamena chyba a vyvolana vyjimka
				Console.WriteLine("Chyba: " + line);
				throw new ApplicationException(line);
			}
			line = ReadLine();  // precteni prazdneho radku
			line = ReadLine();
			while(line != null && !"".Equals(line)){
				raceList.Add(line);
				line = ReadLine();
			}
			return raceList;
		} finally {
			if(socket != null){
				try {
					socket.Close();
				} catch (Exception e) {}
			}
		}
	}
	
	public List<String> listCars(String raceName) {
		List<String> carList = new List<String>();
		Socket socket = null;
		try{
            Connect();			// pripojeni k zavodu
            // pripojeni k zavodu
			outStream.Write("carlist\n");                     // specifikace protokolu
			outStream.Write("race:" + raceName + "\n");
			outStream.Write("\n");
			outStream.Flush();

			// precteni a kontrola dopovedi serveru
			String line = ReadLine();
			if (!line.Equals("ok")) {
				// pokud se pripojeni nepodari, je oznamena chyba a vyvolana vyjimka
				Console.WriteLine("Chyba: " + line);
				throw new ApplicationException(line);
			}
			line = ReadLine();  // precteni prazdneho radku
			line = ReadLine();
			while(line != null && !"".Equals(line)){
				carList.Add(line);
				line = ReadLine();
			}
			return carList;
		} finally {
			if(socket != null){
				try {
					socket.Close();
				} catch (Exception e) {}
			}
		}
	}

	/**
	 * Beh zavodu. Cte data ze serveru. Spousti rizeni auta. 
	 * Ukonci se po ukonceni zavodu.
	 * 
	 * @throws java.io.IOException  problem ve spojeni k serveru
	 */
	private void run() {
		stop = false;
		while (!stop) {							// smycka do konce zavodu
            String line = ReadLine();
//			System.out.println(line);
			if (line.Equals("round")) {			// dalsi kolo v zavode
				round();
			} else if (line.Equals("finish")) {	// konec zavodu konci smucku
				stop = true;
			} else {
				Console.WriteLine("Chyba se serveru: " + line);
			}
		}
	}

	/**
	 * Resi jedno posunuti auta. Precte pozici auta od servru,
	 * vypocte nastaveni rizeni, ktere na server.
	 * 
	 * @throws java.io.IOException   problem ve spojeni k serveru
	 */
	private Dictionary<String, float> values = new Dictionary<String, float>();
	private void round() {
		float angle = 0;     // uhel k care <0,1>
		float speed = 0;     // rychlost auta <0,1>
		float distance0 = 0;  // vzdalenost od cary <0,1>
		float distance4 = 0; // vzdalenost od cary za 4m<0,1>
		float distance8 = 0; // vzdalenost od cary za 8m<0,1>
		float distance16 = 0; // vzdalenost od cary za 16m<0,1>
		float distance32 = 0; // vzdalenost od cary za 32m<0,1>
		float friction = 0;
		float skid = 0;
		float checkpoint = 0;
        float sensorFrontLeft = 0;
        float sensorFrontMiddleLeft = 0;
        float sensorFrontMiddleRight = 0;
        float sensorFrontRight = 0;
        float sensorFrontRightCorner1 = 0;
        float sensorFrontRightCorner2 = 0;
        float sensorRight1 = 0;
        float sensorRight2 = 0;
        float sensorRearRightCorner2 = 0;
        float sensorRearRightCorner1 = 0;
        float sensorRearRight = 0;
        float sensorRearLeft = 0;
        float sensorRearLeftCorner1 = 0;
        float sensorRearLeftCorner2 = 0;
        float sensorLeft1 = 0;
        float sensorLeft2 = 0;
        float sensorFrontLeftCorner1 = 0;
        float sensorFrontLeftCorner2 = 0;
		
		// cteni dat ze serveru
		String line = ReadLine();
//		System.out.println(line);
		values.Clear();
		while (line.Length > 0) {
			String[] data = line.Split(new char[] {':'}, 2);
			String key = data[0];
			String value = data[1];
            values.Add(key, float.Parse(value, CultureInfo.InvariantCulture));
			if (key.Equals("angle")) {
                angle = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("speed")) {
                speed = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("distance0")) {
                distance0 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("distance4")) {
                distance4 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("distance8")) {
                distance8 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("distance16")) {
                distance16 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("distance32")) {
                distance32 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("friction")) {
                friction = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("skid")) {
                skid = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("checkpoint")) {
                checkpoint = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontLeft")) {
                sensorFrontLeft = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontMiddleLeft")) {
                sensorFrontMiddleLeft = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontMiddleRight")) {
                sensorFrontMiddleRight = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontRight")) {
                sensorFrontRight = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontRightCorner1")) {
                sensorFrontRightCorner1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontRightCorner2")) {
                sensorFrontRightCorner2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRight1")) {
                sensorRight1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRight2")) {
                sensorRight2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearRightCorner2")) {
                sensorRearRightCorner2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearRightCorner1")) {
                sensorRearRightCorner1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearRight")) {
                sensorRearRight = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearLeft")) {
                sensorRearLeft = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearLeftCorner1")) {
                sensorRearLeftCorner1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorRearLeftCorner2")) {
                sensorRearLeftCorner2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorLeft1")) {
				sensorLeft1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorLeft2")) {
                sensorLeft2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontLeftCorner1")) {
                sensorFrontLeftCorner1 = float.Parse(value, CultureInfo.InvariantCulture);
			} else if (key.Equals("sensorFrontLeftCorner2")) {
                sensorFrontLeftCorner2 = float.Parse(value, CultureInfo.InvariantCulture);
			} else {
				Console.WriteLine("Chyba se serveru: " + line);
			}
			line = ReadLine();
//			System.out.println(line);
		}

		Dictionary<String, float> responses = driver.drive(values);
		// vypocet nastaveni rizeni, ktery je mozno zmenit za jiny algoritmus

		// odpoved serveru
		outStream.Write("ok\n");
		foreach(String key in responses.Keys){
            outStream.Write(key + ":" + responses[key].ToString(CultureInfo.InvariantCulture) + "\n");
		}
		outStream.Write("\n");
		outStream.Flush();
	}
}
}
