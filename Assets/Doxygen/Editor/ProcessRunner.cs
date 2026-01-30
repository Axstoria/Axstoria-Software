using UnityEditor;
using System.Diagnostics;

namespace ProjectNull.DocTools{
	/*!
	 * \brief Contains a process and this is the interface point.
	 */
	public class ProcessRunner {
		private Process _process;

		public bool ProcessIsActive(){
			if(null!=_process){
				return !_process.HasExited;
			}else{
				return false;
			}
		}

		/*!
		 * \brief Creat a temp working folder and start a process in it
		 */
		public void StartInWorkingFolder(string runFile, string[] arguments){
			//Init the processStartInfo
			ProcessStartInfo processStartInfo=new ProcessStartInfo();

			//Make and setting the working folder
			string workingFolder = FileUtil.GetUniqueTempPathInProject();
			System.IO.DirectoryInfo d = System.IO.Directory.CreateDirectory(workingFolder);
			string fullPathWorkingFolder= d.FullName;
			processStartInfo.WorkingDirectory=fullPathWorkingFolder;

			//The final part of start
			SetupAndStart(runFile,arguments,processStartInfo);

		}

		/*!
		 * \brief Start a process without creating a temp working folder
		 */
		public void StartWitoutWorkingFolder(string runFile, string[] arguments){
			//Init the processStartInfo
			ProcessStartInfo processStartInfo=new ProcessStartInfo();

			//The final part of start
			SetupAndStart(runFile,arguments,processStartInfo);
		}

		/*!
		 * \brief Add name, arbuments and start the process.
		 */
		private void SetupAndStart(string runFile, string[] arguments,ProcessStartInfo processStartInfo){
			//Set the file to run
			processStartInfo.FileName=runFile;
			
			//Set teh arguments
			string allArguments="";
			for (int i=0;i+1<arguments.Length;i++){
				allArguments=allArguments + arguments[i] + " ";
			}
			allArguments=allArguments + arguments[arguments.Length-1];
			processStartInfo.Arguments=allArguments;
			
			//Start the process
			_process = Process.Start(processStartInfo);
		}

		/*!
		 * \brief Get the stream out from the process
		 */
		public System.IO.StreamReader GetOutStream(){
			return _process.StandardOutput;
		}

	}
}
