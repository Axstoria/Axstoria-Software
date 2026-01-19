using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectNull.DocTools{
	public enum DoxyType {
		User,
		Developer
	}

	/*!
	 * \brief The base doxy file class used to make new doxyfiles form a base doxyfile.
	 */	
	public class DoxyFile {

		/*!
		 * \brief Make the doxyfiles in the new folder.
		 */
		public static int MakeTheDoxyFiles(DoxySettings doxySettings){
			int madeDoxy=0;
			madeDoxy = MakeDoxyOfType(DoxyType.User,doxySettings)?1:0;
			madeDoxy = madeDoxy + (MakeDoxyOfType(DoxyType.Developer,doxySettings)?1:0);
			return madeDoxy;
		}

		/*!
		 * \brief Make a doxyfile using a baseDoxy as string and a type string
		 */
		private static bool MakeDoxyOfType(DoxyType doxyType, DoxySettings doxySettings){
			if(!DoxygenFileExist(doxyType, doxySettings)){
				string baseDoxy = ReadBaseConfig();
				string doxyContent=EditbaseDoxyString(baseDoxy, doxyType, doxySettings);
				BuildFile(doxyContent, doxyType, doxySettings);
				return true;
			}else{
				string baseDoxy = ReadDoxyfile(doxyType, doxySettings);
				string doxyContent=EditDoxyString(baseDoxy, doxyType, doxySettings);
				BuildFile(doxyContent, doxyType, doxySettings);
				return false;
			}
		}

		/*!
		 * \breif Test if dxygen file exist
		 */
		public static bool DoxygenFileExist(DoxyType doxyType, DoxySettings doxySettings){
			string activeDocumentationFolder = doxySettings.activeDocumentationFolder.Replace('/','\\');
			if(File.Exists(activeDocumentationFolder + @"\" + doxyType.ToString() + "Doxyfile")){
				return true;
			}else{
				return false;
			}
		}

		/*!
		 * \brief Read the base doxy file to the string baseFileString
		 */
		private static string ReadBaseConfig()
		{
			TextAsset basefile = (TextAsset)Resources.Load("BaseDoxyfile", typeof(TextAsset));
			StringReader reader = new StringReader(basefile.text);
			if ( reader == null ){
				UnityEngine.Debug.LogError("BaseDoxyfile not found or not readable");
				return null;
			}else{
				return reader.ReadToEnd();
			}
		}

		/*!
		 * \brief Read a doxy file acording to doxySettings acoding to doxytype
		 */
		private static string ReadDoxyfile(DoxyType doxyType, DoxySettings doxySettings)
		{
			string doxyFileName = doxySettings.activeDocumentationFolder + @"\" + doxyType + "Doxyfile";
			try
			{
				if(File.Exists(doxyFileName)){
					// Read in non-existent file.
					using (StreamReader sr = new StreamReader(doxyFileName))
					{
						string line = sr.ReadToEnd();
						return line;
					}
				}else{
					return "";
				}
			}catch (FileNotFoundException ex){
				Debug.Log(ex);
			}
			return "";
		}

		/*!
		 * \brief Edit the doxy file and return the compleat string.  
		 */
		private static string EditbaseDoxyString(string baseDoxy,DoxyType doxyType, DoxySettings doxySettings){
			if(baseDoxy!=null){
				//Make the new doxyfile content
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_NUMBER         =[A-Za-z0-9_. ]*","PROJECT_NUMBER         = " + doxySettings.projectVersionNumber);
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_BRIEF          =[A-Za-z0-9_. "+"\""+"]*","PROJECT_BRIEF          = " + "\""+doxySettings.projectBrief+"\"");
				baseDoxy = Regex.Replace(baseDoxy,@"INPUT                  =[A-Za-z0-9_.: /"+"\""+"]*","INPUT                  = " + "\""+doxySettings.projecSourceFolder+"\"");
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_NAME           =[A-Za-z0-9_. "+"\""+"]*","PROJECT_NAME           = " + "\""+doxySettings.projectName+" " + doxyType + " manual\"");
				baseDoxy = Regex.Replace(baseDoxy,@"OUTPUT_DIRECTORY       =[A-Za-z0-9_. \\]*","OUTPUT_DIRECTORY       = " + ".\\" + doxyType + "PDF");
				return baseDoxy;
			}else{
				Debug.Log("Could not build file due to null input");
				return null;
			}
		}

		/*!
		 * \brief Edit the doxy file and return the compleat string.  
		 */
		private static string EditDoxyString(string baseDoxy,DoxyType doxyType, DoxySettings doxySettings){
			if(baseDoxy!=null){
				//Make the new doxyfile content
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_NUMBER         =[A-Za-z0-9_. ]*","PROJECT_NUMBER         = " + doxySettings.projectVersionNumber);
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_BRIEF          =[A-Za-z0-9_. "+"\""+"]*","PROJECT_BRIEF          = " + "\""+doxySettings.projectBrief+"\"");
				baseDoxy = Regex.Replace(baseDoxy,@"PROJECT_NAME           =[A-Za-z0-9_. "+"\""+"]*","PROJECT_NAME           = " + "\""+doxySettings.projectName+" " + doxyType + " manual\"");
				baseDoxy = Regex.Replace(baseDoxy,@"OUTPUT_DIRECTORY       =[A-Za-z0-9_. \\]*","OUTPUT_DIRECTORY       = " + ".\\" + doxyType + "PDF");
				return baseDoxy;
			}else{
				Debug.Log("Could not build file due to null input");
				return null;
			}
		}
		
		/*!
		 * \brief Build the new doxyfile form a sting containing the content
		 */
		private static void BuildFile(string doxyContent, DoxyType doxyType, DoxySettings doxySettings){
			if(doxyContent!=null){
				StringBuilder sb = new StringBuilder();
				sb.Append(doxyContent);
				StreamWriter NewDoxyfile = new StreamWriter(doxySettings.activeDocumentationFolder + @"\" + doxyType + "Doxyfile");
				
				NewDoxyfile.Write(sb.ToString());
				NewDoxyfile.Close();
			}else{
				Debug.Log("Could not build file due to null input");
			}
		}
	}
}
