using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace ProjectNull.DocTools{
	/*!
	 * \brief This is the settings of the 
	 */
	public struct DoxySettings {

		/*!
		 * \breif The path to the doxygen.exe file
		 */
		public string fullDoxygenPath{
			get;set;
		}

		/*!
		 * \breif The path to the active documentation folder
		 * 
		 * It's here the doxy files and PDFs are
		 */
		public string activeDocumentationFolder{
			get;set;
		}

		/*!
		 * \breif The path to the source folder when generating new doxy files.
		 */
		public string projecSourceFolder{
			get;set;
		}

		/*!
		 * \breif The project name to insert in the doxyfile.
		 */
		public string projectName{
			get;set;
		}

		/*!
		 * \breif The project brief description to insert in the doxyfile.
		 */
		public string projectBrief{
			get;set;
		}

		/*!
		 * \breif The project number (version number) to insert in the doxyfile.
		 */
		public string projectVersionNumber{
			get;set;
		}

		/*!
		 * \brief Save the data to the EditorPrefs using SetString
		 */
		public void Save(){
			string projectID = PlayerSettings.productName+"dox";
			EditorPrefs.SetString("fullDoxygenPath",fullDoxygenPath);
			EditorPrefs.SetString(projectID+"activeDocumentationFolder",activeDocumentationFolder);
			EditorPrefs.SetString(projectID+"projectName",projectName);
			EditorPrefs.SetString(projectID+"projectBrief",projectBrief);
			EditorPrefs.SetString(projectID+"projectNumber",projectVersionNumber);
			EditorPrefs.SetString(projectID+"sourceFolder",projecSourceFolder);
		}

		/*!
		 * \brief Used to load the settings from EditorPrefs.
		 */
		public void Load(){

			LoadActiveDocumentationFolderFromEditorPrefs();
			LoadFullDoxygenPathFromEditorPrefs();
			LoadSettingsFromUserDoxyFile();
		}

		/*!
		 * \brief Used to load the settings from EditorPrefs.
		 */
		public void LoadFromEditorPrefs(){
			LoadActiveDocumentationFolderFromEditorPrefs();
			LoadNameFromEditorPrefs();
			LoadBriefFromEditorPrefs();
			LoadVersionFromEditorPrefs();
			LoadFullDoxygenPathFromEditorPrefs();
			LoadSourceFolderFromEditorPrefs();

		}



		private void LoadActiveDocumentationFolderFromEditorPrefs(){
			string projectID = PlayerSettings.productName+"dox";
			activeDocumentationFolder=EditorPrefs.GetString(projectID+"activeDocumentationFolder");
			if(null==activeDocumentationFolder){ activeDocumentationFolder="";}
		}

		private void LoadFullDoxygenPathFromEditorPrefs(){
			fullDoxygenPath=EditorPrefs.GetString("fullDoxygenPath");
			if(null==fullDoxygenPath){ fullDoxygenPath="";}
		}

		private void LoadNameFromEditorPrefs(){
			string projectID = PlayerSettings.productName+"dox";
			projectName=EditorPrefs.GetString(projectID+"projectName");
			if(null==projectName){ projectName="";}
		}

		private void LoadBriefFromEditorPrefs(){
			string projectID = PlayerSettings.productName+"dox";
			projectBrief=EditorPrefs.GetString(projectID+"projectBrief");
			if(null==projectBrief){ projectBrief="This is my project";}
		}

		private void LoadVersionFromEditorPrefs(){
			string projectID = PlayerSettings.productName+"dox";
			projectVersionNumber=EditorPrefs.GetString(projectID+"projectNumber");
			if(null==projectVersionNumber){ projectVersionNumber="1.0";}
		}

		private void LoadSourceFolderFromEditorPrefs(){
			string projectID = PlayerSettings.productName+"dox";
			projecSourceFolder=EditorPrefs.GetString(projectID+"sourceFolder");
			if(null==projecSourceFolder){ projecSourceFolder="";}
		}

		/*!
		 * \brief Loading settings from the doxygen using the user doxygen file.
		 */
		public void LoadSettingsFromUserDoxyFile(){
			string userDoxyFile = activeDocumentationFolder + "/UserDoxyfile";
			if(File.Exists(userDoxyFile)){
				using (StreamReader sr = new StreamReader(userDoxyFile))
				{
					string line = sr.ReadToEnd();

					tryToLoadName(line);
					tryToLoadBrief(line);
					tryToLoadVersion(line);
					tryToSourceFolder(line);
				}
			}else{
				LoadFromEditorPrefs();
			}
		}

		private void tryToLoadName(string line){
			Group name = Regex.Match(line,@"PROJECT_NAME           = " + "\""+ @"(?<word>[A-Za-z0-9_.\s]*)"+ "\"").Groups["word"];
			if(1==name.Captures.Count){
				string nameToBeEdited=name.Value;
				projectName=nameToBeEdited.Replace(" User manual","");
			}else{
				LoadNameFromEditorPrefs();
			}
		}

		private void tryToLoadBrief(string line){
			Group brief = Regex.Match(line,@"PROJECT_BRIEF          = " + "\""+ @"(?<word>[A-Za-z0-9_.\s]*)"+ "\"").Groups["word"];
			if(1==brief.Captures.Count){
				projectBrief=brief.Value;
			}else{
				LoadBriefFromEditorPrefs();
			}
		}

		private void tryToLoadVersion(string line){
			Group version= Regex.Match(line,@"PROJECT_NUMBER         = (?<number>[A-Za-z0-9_.]*)").Groups["number"];
			if(1==version.Captures.Count){
				projectVersionNumber=version.Value;
			}else{
				LoadVersionFromEditorPrefs();
			}
		}

		private void tryToSourceFolder(string line){
			Group source= Regex.Match(line,@"INPUT                  = " + "\""+ @"(?<source>[A-Za-z0-9_.:/]*)" + "\"").Groups["source"];
			if(1==source.Captures.Count){
				projecSourceFolder=source.Value;
			}else{
				LoadSourceFolderFromEditorPrefs();
			}
		}
	}
}
