using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace ProjectNull.DocTools{
	/*!
	 * \brief The doxygen window in the editor to access doxygen and the documentation.
	 */
	public class DoxyWindow : EditorWindow {

		private int _tabOpen=2;
		private string _msgToUser="";
		private DoxySettings _doxySettings=new DoxySettings();
		private Vector2 scroll=new Vector2();
		private ProcessRunner curentProcess=null;

		/*!
		 * \breif Load doxySettings from the UserDoxyFile of ot exist.
		 * 
		 * If The user doxy file do not exist load from editor pref. or
		 * as last restort default.
		 */
		void OnEnable()
		{
			_doxySettings.Load();
			if(DoxyFile.DoxygenFileExist(DoxyType.User, _doxySettings)){
				_msgToUser="User doxy file found. Settings load from the UserDoxyfile.";
			}else{
				_msgToUser="User doxy file not found. Settings loaded from Editor Pref.";
			}
		}

		/*!
		 * \brief The menu item
		 */
		[MenuItem("Window/Doxygen documentation tool")]
		private static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow w= EditorWindow.GetWindow(typeof(DoxyWindow),false,"DoxygenTool");
		}



		/*!
		 * \brief The internal callback
		 */
		private void OnGUI()
		{
			TabMenu();
			scroll = GUILayout.BeginScrollView(scroll);

			DisplayMsgToUser();

			switch(_tabOpen){
			case 1:
				SettingGUI();
				break;


			case 2:
				GenerateViewEditGUI();
				break;
			}

			GUILayout.EndScrollView();

		}

		/*!
		 * \brief The user mesage from the doc tool
		 */
		private void DisplayMsgToUser(){
			if(""!=_msgToUser){
				GUIStyle myStyle= new GUIStyle();
				myStyle.normal.textColor=Color.red;
				myStyle.wordWrap=true;
				GUILayout.Label(_msgToUser,myStyle);
			}
		}

		/*!
		 * \brief The menu at the top of the window
		 */
		private void TabMenu()
		{
			GUILayout.BeginHorizontal( EditorStyles.toolbar );
			{
				if( GUILayout.Toggle( 2 == _tabOpen, "Generate/View/Edit", EditorStyles.toolbarButton ) )
				{
					NavigateTo(2);
				}
				if( GUILayout.Toggle( 1 == _tabOpen, "Settings", EditorStyles.toolbarButton ) )
				{
					NavigateTo(1);
				}
			}
			GUILayout.EndHorizontal();
		}

		/*!
		 * \brief Used to navigate between tabs
		 */
		private void NavigateTo(int tabID){
			if(_tabOpen!=tabID){
				_tabOpen=tabID;
				_msgToUser="";
				scroll=new Vector2();
			}
		}

		/*!
		 * \brief The settings tab
		 */
		private void SettingGUI(){
			if(GUILayout.Button ("Save settings (and build DoxyFile if needed)", EditorStyles.miniButton))
			{
				_doxySettings.Save();
				int madeDoxy = DoxyFile.MakeTheDoxyFiles(_doxySettings);
				if(2==madeDoxy){
					_msgToUser="Settings saved and two doxyfile produced.";
				}else if(1==madeDoxy){
					_msgToUser="Settings saved and one doxyfile produced.";
				}else{
					_msgToUser="Settings saved";
				}
			}


			//Documentation directory
			GUILayout.Space (10);
			GUILayout.Label("The path to the active documentation folder",EditorStyles.wordWrappedLabel);
			GUILayout.BeginHorizontal();
			_doxySettings.activeDocumentationFolder = EditorGUILayout.TextField("Documentation : ",_doxySettings.activeDocumentationFolder);
			if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22))){
				_doxySettings.activeDocumentationFolder = EditorUtility.OpenFolderPanel("Choose the documentation folder",_doxySettings.activeDocumentationFolder, "");
				if(""!=_doxySettings.activeDocumentationFolder){
					string userDoxyFile = _doxySettings.activeDocumentationFolder + "/UserDoxyfile";
					if(File.Exists(userDoxyFile)){
						_doxySettings.LoadSettingsFromUserDoxyFile();
					}
				}
			}
			GUILayout.EndHorizontal();

			//Reload doxygen file
			if(GUILayout.Button("Reload settings from UserDoxygen file", EditorStyles.miniButton)){
				_doxySettings.LoadSettingsFromUserDoxyFile();
				if(DoxyFile.DoxygenFileExist(DoxyType.User, _doxySettings)){
					_msgToUser="User doxy file found. Settings load from the UserDoxyfile.";
				}else{
					_msgToUser="User doxy file not found. Settings loaded from Editor Pref.";
				}
			}

			//Doxy file settings
			GUILayout.Space (10);
			GUILayout.Label("The following settings is loaded \nfrom UserDoxyfile if doxyfile exist!",EditorStyles.boldLabel);

			//Doxy project settings
			GUILayout.Label("Provide some details about the project to be inserted in the doxyfile",EditorStyles.wordWrappedLabel);
			_doxySettings.projectName = EditorGUILayout.TextField("Project Name: ",_doxySettings.projectName);
			_doxySettings.projectBrief = EditorGUILayout.TextField("Short Description: ",_doxySettings.projectBrief);
			_doxySettings.projectVersionNumber = EditorGUILayout.TextField("Version: ",_doxySettings.projectVersionNumber);

			//Doxy path
			GUILayout.Space (10);
			GUILayout.Label("The path to doxygen.exe",EditorStyles.wordWrappedLabel);
			EditorGUILayout.BeginHorizontal();
			_doxySettings.fullDoxygenPath = EditorGUILayout.TextField("Doxygen.exe : ",_doxySettings.fullDoxygenPath);
			if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22))){
				_doxySettings.fullDoxygenPath = EditorUtility.OpenFilePanel("Where is doxygen.exe installed?",_doxySettings.fullDoxygenPath, "");
			}
			EditorGUILayout.EndHorizontal();

			//Sours directory
			GUILayout.Label("This settings is only used \nif new doxy file is created!",EditorStyles.boldLabel);
			GUILayout.Space (10);
			GUILayout.Label("The path to root of the source",EditorStyles.wordWrappedLabel);
			EditorGUILayout.BeginHorizontal();
			_doxySettings.projecSourceFolder = EditorGUILayout.TextField("Source : ",_doxySettings.projecSourceFolder);
			if(GUILayout.Button ("...",EditorStyles.miniButtonRight, GUILayout.Width(22))){
				_doxySettings.projecSourceFolder = EditorUtility.OpenFolderPanel("Choose the root of the source",_doxySettings.projecSourceFolder, "");
			}
			EditorGUILayout.EndHorizontal();
		}

		/*!
		 * \brief The Generate View Edit tab
		 */
		private void GenerateViewEditGUI(){
			GUILayout.BeginVertical();
			if(GUILayout.Button("Generate PDFs")){
				TryToGeneratePDF();
			}
			GUILayout.Space(10);
			if(GUILayout.Button("Browse User Documentation")){
				BrowsPDF(DoxyType.User);
			}
			if(GUILayout.Button("Browse Developer Documentation")){
				BrowsPDF(DoxyType.Developer);
			}
			GUILayout.Space(10);
			if(GUILayout.Button("Edit User Documentation")){
				EditDoxygen(DoxyType.User);
			}
			if(GUILayout.Button("Edit Developer Documentation")){
				EditDoxygen(DoxyType.Developer);
			}
			GUILayout.EndVertical();
		}

		private void BrowsPDF(DoxyType doxyType){
			string s=_doxySettings.activeDocumentationFolder;
			s=s.Replace('/','\\');
			s=s+"\\" + doxyType.ToString() + "Documentation.pdf";
			if(File.Exists(@s)){
				_msgToUser="";
				System.Diagnostics.Process.Start(@s);
			}else{
				_msgToUser = doxyType.ToString() + "Documentation.pdf where not found in " + @s;
			}
		}

		private void EditDoxygen(DoxyType doxyType){

			string arg=_doxySettings.activeDocumentationFolder;
			arg=arg.Replace('/','\\');
			arg=arg+"\\" + doxyType.ToString() + "Doxyfile";
			
			string doxyWizard=_doxySettings.fullDoxygenPath.Replace("doxygen.exe","doxywizard.exe");
			doxyWizard=doxyWizard.Replace('/','\\');


			if(File.Exists(doxyWizard)&&File.Exists(arg)){
				_msgToUser="";
				System.Diagnostics.Process.Start("\""+ doxyWizard + "\"","\""+ arg +"\"");
			}else{
				_msgToUser="";
				if(!File.Exists(doxyWizard)){
					_msgToUser = "doxywizard.exe is not found at:" + doxyWizard;
				}
				if(!File.Exists(arg)){
					_msgToUser += "Did not find the doxyfile at:" + arg;
				}
			}

		}
		
		private void TryToGeneratePDF(){
			if((null==curentProcess?true:!(curentProcess.ProcessIsActive()))){
				GeneratePDF();
			}else{
				_msgToUser="Can't generate PDF. One process is alrady running.";
			}
		}

		private void GeneratePDF(){
			//Documetnation folder
			string documentationFolder=_doxySettings.activeDocumentationFolder.Replace('/','\\');

			string doxygenPath = _doxySettings.fullDoxygenPath.Replace('/', '\\');


			//Find the batch file in the project.
			string curentFolder = Directory.GetCurrentDirectory();
			curentFolder=curentFolder.Replace('/','\\');
			List<string> batFile= new List<string>();
			RecursiveSerch(curentFolder,"DoxyPDF.bat",batFile);

			if(DoxyFilesInPlace()){
				curentProcess = new ProcessRunner();
				curentProcess.StartInWorkingFolder(batFile[0],new string[]{"\"" + documentationFolder + "\"" + " \"" + doxygenPath + "\""  });
			}
		}

		/*!
		 * \breif Verify that the doxyfiles are present
		 */
		private bool DoxyFilesInPlace(){
			string activeDocumentationFolder = _doxySettings.activeDocumentationFolder.Replace('/','\\');
			_msgToUser="";
			bool allFine=true;

			if(!File.Exists(activeDocumentationFolder + @"\UserDoxyfile")){
				allFine=false;
				_msgToUser +="\nUserDoxyfile is missing at: " + activeDocumentationFolder + @"\UserDoxyfile";
			}
			if(!File.Exists(activeDocumentationFolder + @"\UserDoxyfile")){
				allFine=false;
				_msgToUser +="\nDeveloperDoxyfile is missing at: " + activeDocumentationFolder + @"\UserDoxyfile";
			}
			if(!File.Exists(_doxySettings.fullDoxygenPath)){
				allFine=false;
				_msgToUser +="\ndoxygen.exe is missing at: " +_doxySettings.fullDoxygenPath;
			}

			if(false==allFine){
				_msgToUser = "Can not make PDF:" +_msgToUser;
				return false;
			}else{
				_msgToUser="";
				return true;

			}
		}
		
		/*!
		 * \brief Used to find files in a filepath
		 */
		List<string> RecursiveSerch(string dir, string filePattern ,List<string> returnList){
			try	
			{
				foreach (string d in Directory.GetDirectories(dir)) 
				{
					foreach (string f in Directory.GetFiles(d, filePattern)) 
					{
						returnList.Add(f);
					}
					returnList = RecursiveSerch(d,filePattern, returnList);
				}
			}
			catch (System.Exception excpt) 
			{
				Debug.Log(excpt.Message);
			}
			return returnList;
		}



	}
}
