using System;
using Tamir.SharpSsh;
using System.Threading;

namespace sharpSshTest
{
	/// <summary>
	/// Summary description for sharpSshTest.
	/// </summary>
	public class sharpSshTest
	{
		static string host, user, pass;
		public static void Main()
		{
			PrintVersoin();
			Console.WriteLine();
			Console.WriteLine("1) Simple SSH session example using SshStream");
			Console.WriteLine("2) SCP example from local to remote");
			Console.WriteLine("3) SCP example from remote to local");
			Console.WriteLine();

			INPUT:
			int i=-1;
			Console.Write("Please enter your choice: ");
			try
			{
				i = int.Parse( Console.ReadLine() );
				Console.WriteLine();				
			}
			catch
			{
				i=-1;
			}

			switch(i)
			{
				case 1:
					SshStream();
					break;
				case 2:
					Scp("to");
					break;
				case 3:
					Scp("from");
					break;
				default:
					Console.Write("Bad input, ");
					goto INPUT;
			}
		}

		/// <summary>
		/// Get input from the user
		/// </summary>
		public static void GetInput()
		{
			Console.Write("Remote Host: ");
			host = Console.ReadLine();
			Console.Write("User: ");
			user = Console.ReadLine();
			Console.Write("Password: ");
			pass = Console.ReadLine();
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates the SshStream class
		/// </summary>
		public static void SshStream()
		{
			GetInput();

			try
			{			
				Console.Write("-Connecting...");
				SshStream ssh = new SshStream(host, user, pass);
				Console.WriteLine("OK ({0}/{1})",ssh.Cipher,ssh.Mac);
				Console.WriteLine("Server version={0}, Client version={1}", ssh.ServerVersion, ssh.ClientVersion);
				Console.WriteLine("-Use the 'exit' command to disconnect.");
				Console.WriteLine();

				//Sets the end of response character
				ssh.Prompt = "#";
				//Remove terminal emulation characters
				ssh.RemoveTerminalEmulationCharacters = true;

				//Reads the initial response from the SSH stream
				Console.Write( ssh.ReadResponse() );

				//Send commands from the user
				while(true)
				{
					string command = Console.ReadLine();
					if (command.ToLower().Equals("exit"))
						break;

					//Write command to the SSH stream
					ssh.Write( command );
					//Read response from the SSH stream
					Console.Write( ssh.ReadResponse() );
				}
				ssh.Close(); //Close the connection
				Console.WriteLine("Connection closed.");
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// Demonstrates the Scp class
		/// </summary>
		/// <param name="cmd">Either "to" or "from"</param>
		public static void Scp(string cmd)
		{
			GetInput();

			string local=null, remote=null;

			if(cmd.ToLower().Equals("to"))
			{
				Console.Write("Local file: ");
				local = Console.ReadLine();
				Console.Write("Remote file: ");
				remote = Console.ReadLine();
			}
			else if(cmd.ToLower().Equals("from"))
			{
				Console.Write("Remote file: ");
				remote = Console.ReadLine();
				Console.Write("Local file: ");
				local = Console.ReadLine();
			}

			Scp scp = new Scp();
			scp.OnConnecting += new FileTansferEvent(scp_OnConnecting);
			scp.OnStart += new FileTansferEvent(scp_OnProgress);
			scp.OnEnd += new FileTansferEvent(scp_OnEnd);
			scp.OnProgress += new FileTansferEvent(scp_OnProgress);

			try
			{
				if(cmd.ToLower().Equals("to"))
					scp.To(local, host, remote, user, pass);
				else if(cmd.ToLower().Equals("from"))
					scp.From(host, remote, user, pass,local);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		static void PrintVersoin()
		{
			try
			{				
				System.Reflection.Assembly asm
					= System.Reflection.Assembly.GetAssembly(typeof(Tamir.SharpSsh.SshStream));
				Console.WriteLine("sharpSsh v"+asm.GetName().Version);
			}
			catch
			{
				Console.WriteLine("sharpSsh v1.0");
			}
		}

		#region SCP Event Handlers

		static ConsoleProgressBar progressBar;

		private static void scp_OnConnecting(int transferredBytes, int totalBytes, string message)
		{
			Console.WriteLine();
			progressBar = new ConsoleProgressBar();
			progressBar.Update(transferredBytes, totalBytes, message);
		}

		private static void scp_OnProgress(int transferredBytes, int totalBytes, string message)
		{
			progressBar.Update(transferredBytes, totalBytes, message);
		}

		private static void scp_OnEnd(int transferredBytes, int totalBytes, string message)
		{
			progressBar.Update(transferredBytes, totalBytes, message);
			progressBar=null;
		}

		#endregion SCP Event Handlers


	}
}
