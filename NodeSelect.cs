using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.IO;
//using NodeSelect;
/* 

Copyright (c) 2013, Matti 'voneiden' Eiden
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

v1.2 
- Keep open tickbox to keep window open even if node closes
- Patched conics buttons
- Lots of new buttons

v1.1
- Total delta-v displayed

v1.01
- On the fly patched conics changer

v1.0
- Added helper colors to the GUI
- Renamed and reordered the vector list
- Squashed a bug that caused the manual edits to be lost when gizmo was adjusted by mouse

 */

namespace NodeSelect  {

	[KSPAddon(KSPAddon.Startup.Flight, false) ]
	public class NodeSelect : MonoBehaviour
	{
		public static GameObject GameObjectInstance;

		public PluginConfiguration cfg;
		private bool cfgLoaded = false;
		public Dictionary<ManeuverNode,PatchedConicRenderer> D1 = new Dictionary<ManeuverNode,PatchedConicRenderer>();
		public Dictionary<ManeuverNode,PatchRendering> D2 = new Dictionary<ManeuverNode,PatchRendering>();

		public ManeuverNode activeNode = null;
		public ManeuverNode lastActiveNode = null;
		private Rect windowPosition = new Rect(Screen.width / 6*3, 10, 260, 250);

		//private bool bClose = false;
		//private bool bHidden = false;

		private string nsT = "0";
		private string nsX = "0";
		private string nsY = "0";
		private string nsZ = "0";
		//private string conicsMode = "3";
		private KeyCode cycleKey;
		private KeyCode hideKey;

		private bool keepOpen = false;


		// This are the variables for mouse adjusting the node
		private bool bTp = false;
		private bool bTa = false;
		private string sTa = "1";
		private bool bTm = false;
		private bool bTs = false;

		private bool bPp = false;
		private bool bPa = false;
		private string sPa = "1";
		private bool bPm = false;
		private bool bPs = false;

		private bool bRp = false;
		private bool bRa = false;
		private string sRa = "1";
		private bool bRm = false;
		private bool bRs = false;

		private bool bNp = false;
		private bool bNa = false;
		private string sNa = "1";
		private bool bNm = false;
		private bool bNs = false;

		// Patched conics buttons
		private bool bC0 = false;
		private bool bC1 = false;
		private bool bC2 = false;
		private bool bC3 = false;
		private bool bC4 = false;

		// Max patched conics buttons
		//private bool bMC_minus = false;
		//private bool bMC_plus = false;
		//private string bMC = FlightGlobals.ActiveVessel.patchedConicRenderer.
		// private double rad2deg = (180.0 / Math.PI);

		//private static Core _core;
		//private static bool _draw = true;
		
		public void Awake ()
		{
			DontDestroyOnLoad (this);
			CancelInvoke ();
			//InvokeRepeating ("ClockUpdate",0.2F,0.2F);
			Debug.Log ("NodeFreeze is alive");

			if (cfgLoaded == false) {
				Debug.Log ("Enabling cfg file");
				cfg = KSP.IO.PluginConfiguration.CreateForType<NodeSelect>(null);
				Debug.Log ("Loading cfg");
				cfg.load ();
				Debug.Log ("CFG loaded");
				cfgLoaded = true;
				try {
					string cfgCycleKey = cfg.GetValue<String> ("cycleKey", "undefined");
					if (cfgCycleKey.Equals ("undefined")) {
						cfg ["cycleKey"] = "O";
						cfgCycleKey = "O";
						cfg.save ();
					}
					cycleKey = (KeyCode)Enum.Parse (typeof(KeyCode), "O");
					Debug.Log ("Cycle key set through try");
					string cfgConicsMode = cfg.GetValue<String> ("conicsMode", "3");
				} catch (ArgumentException) {
					cycleKey = KeyCode.O;
					Debug.Log ("Cycle key set through exception");
				}

				// Setting hide key
				/*
				try {
					string cfgHideKey = cfg.GetValue<String> ("hideKey", "undefined");
					if (cfgHideKey.Equals ("undefined")) {
						cfg ["hideKey"] = "I";
						cfgHideKey = "I";
						cfg.save ();
					}
					hideKey = (KeyCode)Enum.Parse (typeof(KeyCode), cfgHideKey);
					Debug.Log ("Hide key set through try");
				} catch (ArgumentException) {
					hideKey = KeyCode.I;
					Debug.Log ("Hide key set through exception");
				}
				*/
			}
		}
		//public void ClockUpdate ()
		//{
		//	if (IsEnabled && _draw) {
		//		_core.ClockUpdate ();
		//	}
		//}
		//public void OnGUI()
		//{
		//	if (IsEnabled && _draw)
		//		Window.DrawAll();
		//}
		
		public void Update ()
		{
			if (IsEnabled) {
				CheckForGizmos();
				if (Input.GetKeyDown (cycleKey))
				{
					EnableGizmo ();
					/*
					if (backupNode != null && backupNode.attachedGizmo == null) {
						backupNode.AttachGizmo (MapView.ManeuverNodePrefab,backupRenderer,backupPR);
						Debug.Log ("Reattached gizmo");
					}
					*/

				}
				//if (Input.GetKeyDown (hideKey)) {
				//	bHidden = !bHidden;
				//}
				// TODO: make it better
				/*
				try {
					PatchRendering.RelativityMode mode = (PatchRendering.RelativityMode) Enum.Parse(typeof(PatchRendering.RelativityMode),conicsMode);
					if (mode != FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode ) {
						cfg["conicsMode"] = mode.ToString ();
						FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = mode;
						cfg.save ();
					}
				}
				catch (ArgumentException){
				//if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode. != Convert.ToIntconicsMode))
					Debug.Log ("Invalid conicsMode");
				}
				*/
			}
		}
		public void OnGUI ()
		{
			if ((activeNode != null || keepOpen) && IsEnabled) {
				drawGUI();
			}
		}
		public void EnableGizmo ()
		{
			foreach (var pair in D1) {
				// Check for removed nodes
				if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Contains (pair.Key) == false) {
					//Debug.Log ("Removing node");
					D1.Remove (pair.Key);
					D2.Remove (pair.Key);
					EnableGizmo (); // Do the function again, since you shouldn't modify a list while iterating through it..
					return;
				}
			}
			if (D1.Keys.Count == 0) { // Nothing we can do here
				activeNode = null;
				lastActiveNode = null;
				return;
			}
			ManeuverNode newNode = null;

			if (activeNode == null) { // No node selected currently
				if (lastActiveNode != null) { // But there was one previously..
					if (FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Contains (lastActiveNode) == false) {
						// But it doesn't exist anymore, lets just choose the first one on the list
						newNode = D1.Keys.ElementAt (0);
						//Debug.Log ("Node has been destroyed");
					}
					else { // Lucky us, the node still exists..
						newNode = lastActiveNode;
					}
				}
				else {
					newNode = D1.Keys.ElementAt (0);
				}
			}
			else { // There's a node selected currently
				//Debug.Log ("NODE SELECTED ATM");
				if (D1.Keys.Count > 1) { // More than one, we can cycle through them
					//Debug.Log ("CYCLING");
					bool getNext = false;
					ManeuverNode firstNode = null;
					foreach (ManeuverNode mn in D1.Keys) { 
						if (firstNode == null) {
							firstNode = mn;
						}
						if (getNext == true) {
							newNode = mn;
							//Debug.Log ("NEWNODE FOUND");
							break;
						}
						if (mn == activeNode) {
							getNext = true;
							//Debug.Log ("ACTIVE NODE FOUND");
						}
					}
					if (newNode == null) {
						//Debug.Log ("NO NEWNODE FOUND, SELECTING firstNode");
						newNode = firstNode;
					}
					activeNode.DetachGizmo (); // Disable the old gizmo
				}
				else {
					return; // Nothing to do here, only one node and its active
				}
			}
			// Just to be sure, we don't want the newNode to have any open gizmos..
			if (newNode.attachedGizmo != null) {
				newNode.DetachGizmo ();
			}
			newNode.AttachGizmo(MapView.ManeuverNodePrefab,D1[newNode],D2[newNode]);
			activeNode = newNode;
			lastActiveNode = newNode;

			//ndX = newNode.DeltaV.x;
			//ndY = newNode.DeltaV.y;
			//ndZ = newNode.DeltaV.z;
			nsT = newNode.UT.ToString ();
			nsX = newNode.DeltaV.x.ToString ();
			nsY = newNode.DeltaV.y.ToString ();
			nsZ = newNode.DeltaV.z.ToString ();


		}
		public void CheckForGizmos ()
		{
			//Debug.Log ("Backing up ");
			activeNode = null;
			foreach (ManeuverNode mn in FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes) {
				if (mn.attachedGizmo != null) {
					if (activeNode == null) {
						activeNode = mn;
						lastActiveNode = mn;
					    if (D1.ContainsKey (mn) == false) {
							D1[mn] = mn.attachedGizmo.renderer;
							D2[mn] = mn.attachedGizmo.pr;
						}
					}
					else { // Multiple active nodes? Better safe than sorry
						mn.DetachGizmo (); // Kill it with fire!!
					}
				}
			} 
			// Keep window open ..
			//if (activeNode == null && lastActiveNode != null) {
			//	activeNode = lastActiveNode;
			//}
		}
		public void drawGUI ()
		{
			GUI.skin = HighLogic.Skin;
			windowPosition = GUILayout.Window (83188,windowPosition,drawWindow,"Maneuver node");
			//GUI.FocusWindow(83188);
		}

		public void drawWindow (int id)
		{
			double ndT;
			double ndX;
			double ndY;
			double ndZ;

			// If mouse events have changed the gizmo, update our string values
			try {
				ndT = Convert.ToDouble (nsT);
				ndX = Convert.ToDouble (nsX);
				ndY = Convert.ToDouble (nsY);
				ndZ = Convert.ToDouble (nsZ);

			} 
			catch (FormatException) {
				ndT = lastActiveNode.UT;
				ndX = lastActiveNode.DeltaV.x;
				ndY = lastActiveNode.DeltaV.y;
				ndZ = lastActiveNode.DeltaV.z;

				//Debug.Log ("Invalid chars in menu");
			}

			if (ndT != lastActiveNode.UT) {
				nsT = lastActiveNode.UT.ToString ();
			}

			if (ndX != lastActiveNode.DeltaV.x) {
				nsX = lastActiveNode.DeltaV.x.ToString ();
			}

			if (ndY != lastActiveNode.DeltaV.y) {
				nsY = lastActiveNode.DeltaV.y.ToString ();
			}

			if (ndZ != lastActiveNode.DeltaV.z) {
				nsZ = lastActiveNode.DeltaV.z.ToString ();
			}
			
			/*
			Vector3d refV = new Vector3d(1,0,0);
			bool hasAngle = false;
			double angle = 0;
			if (refV.magnitude > 0) {
				angle = Vector3d.Angle (refV,activeNode.nextPatch.getPositionAtT (activeNode.UT));
				hasAngle = true;
			}
			*/
			// Render the window
			Color defbcolor = GUI.backgroundColor;
			//Color defccolor = GUI.contentColor;

			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("UT",GUILayout.Width (30));
			GUI.backgroundColor = Color.yellow;
			nsT = GUILayout.TextField (nsT);
			GUI.backgroundColor = defbcolor;
			GUILayout.Label ("s",GUILayout.Width (30));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			bTm = GUILayout.Button ("-");
			bTa = GUILayout.Button (sTa);
			bTp = GUILayout.Button ("+");
			bTs = GUILayout.Button ("S");
			GUILayout.EndHorizontal ();

			/*
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("θ",GUILayout.Width (30));
			GUILayout.FlexibleSpace ();
			if (hasAngle == true) {
				GUILayout.Label ((angle*rad2deg).ToString ()+"°");
			}
			else {
				GUILayout.Label("NaN");
			}
			GUILayout.EndHorizontal ();
			*/
			GUILayout.BeginHorizontal ();
			//GUI.color = Color.green;


			GUILayout.Label ("Pgd",GUILayout.Width (30));
			//GUILayout.FlexibleSpace();
			GUI.backgroundColor = Color.green;
			nsZ = GUILayout.TextField (nsZ,GUILayout.Width (90));
			GUI.backgroundColor = defbcolor;
			GUILayout.Label ("m/s",GUILayout.Width (30));
			bPm = GUILayout.Button ("-");
			bPa = GUILayout.Button (sPa);
			bPp = GUILayout.Button ("+");
			bPs = GUILayout.Button ("S");

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			//GUI.color = Color.blue;
			GUILayout.Label ("Rad",GUILayout.Width (30));
			//GUILayout.FlexibleSpace();
			GUI.backgroundColor = Color.cyan;
			nsX = GUILayout.TextField (nsX,GUILayout.Width (90));
			GUI.backgroundColor = defbcolor;
			GUILayout.Label ("m/s",GUILayout.Width (30));
			bRm = GUILayout.Button ("-");
			bRa = GUILayout.Button (sRa);
			bRp = GUILayout.Button ("+");
			bRs = GUILayout.Button ("S");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Nml",GUILayout.Width (30));
			GUI.backgroundColor = Color.magenta;
			nsY = GUILayout.TextField (nsY,GUILayout.Width (90));
			GUI.backgroundColor = defbcolor;
			GUILayout.Label ("m/s",GUILayout.Width (30));
			bNm = GUILayout.Button ("-");
			bNa = GUILayout.Button (sNa);
			bNp = GUILayout.Button ("+");
			bNs = GUILayout.Button ("S");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Total Δv:",GUILayout.Width (80));
			GUILayout.Label (lastActiveNode.DeltaV.magnitude.ToString ("0.0"));
			GUILayout.Label ("m/s");
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Conics mode",GUILayout.Width (80));
			//GUILayout.FlexibleSpace();
			//GUI.backgroundColor = Color.red;
			//conicsMode = GUILayout.TextField (conicsMode,GUILayout.Width (30));
			GUI.backgroundColor = defbcolor;
			if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode == PatchRendering.RelativityMode.LOCAL_TO_BODIES) {GUI.backgroundColor = Color.red;}
			bC0 = GUILayout.Button ("0");
			GUI.backgroundColor = defbcolor;
			if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode == PatchRendering.RelativityMode.LOCAL_AT_SOI_ENTRY_UT) {GUI.backgroundColor = Color.red;}
			bC1 = GUILayout.Button ("1");
			GUI.backgroundColor = defbcolor;
			if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode == PatchRendering.RelativityMode.LOCAL_AT_SOI_EXIT_UT) {GUI.backgroundColor = Color.red;}
			bC2 = GUILayout.Button ("2");
			GUI.backgroundColor = defbcolor;
			if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode == PatchRendering.RelativityMode.RELATIVE) {GUI.backgroundColor = Color.red;}
			bC3 = GUILayout.Button ("3");
			GUI.backgroundColor = defbcolor;
			if (FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode == PatchRendering.RelativityMode.DYNAMIC) {GUI.backgroundColor = Color.red;}
			bC4 = GUILayout.Button ("4");
			GUI.backgroundColor = defbcolor;
			//GUILayout.Label ("Total conics",GUILayout.Width (80));
			//GUILayout.FlexibleSpace();
			//GUI.backgroundColor = Color.red;
			//totalConics = GUILayout.TextField (totalConics,GUILayout.Width (30));
			//GUI.backgroundColor = defbcolor;
			GUILayout.EndHorizontal ();



			GUILayout.BeginHorizontal ();
			keepOpen = GUILayout.Toggle (keepOpen,"Keep open");
			GUILayout.EndHorizontal ();



			//FlightGlobals.ActiveVessel.patchedConicRenderer
			GUILayout.EndVertical ();
			GUI.DragWindow ();

			//GUI.contentColor = defccolor;
			GUI.backgroundColor = defbcolor;

			// Update flight plan from window elements

			// Time buttons
			// Prograde buttons
			if (bTp) {
				Decimal i = Convert.ToDecimal (sTa);
				if (Math.Abs (i) < (decimal) 1000) { 
					i = i*(decimal)10;
					sTa = i.ToString ("0.#####");
				}
			}
			if (bTm) {
				Decimal i = Convert.ToDecimal (sTa);
				if (Math.Abs (i) > (decimal) 0.0001) { 
					i = i/(decimal)10;
					sTa = i.ToString ("0.#####");
				}
			}
			if (bTs) {
				sTa = (-Convert.ToDecimal (sTa)).ToString ();
			}
			if (bTa) {
				ndT = Convert.ToDouble (sTa) + Convert.ToDouble (nsT);
				nsT = ndT.ToString ("0.#####");
			}


			// Prograde buttons
			if (bPp) {
				Decimal i = Convert.ToDecimal (sPa);
				if (Math.Abs (i) < (decimal) 1000) { 
					i = i*(decimal)10;
					sPa = i.ToString ("0.#####");
				}
			}
			if (bPm) {
				Decimal i = Convert.ToDecimal (sPa);
				if (Math.Abs (i) > (decimal) 0.0001) { 
					i = i/(decimal)10;
					sPa = i.ToString ("0.#####");
				}
			}
			if (bPs) {
				sPa = (-Convert.ToDecimal (sPa)).ToString ();
			}
			if (bPa) {
				ndZ = Convert.ToDouble (sPa) + Convert.ToDouble (nsZ);
				nsZ = ndZ.ToString ("0.#####");
			}

			// Radial buttons
			if (bRp) {
				Decimal i = Convert.ToDecimal (sRa);
				if (Math.Abs (i) < (decimal) 1000) { 
					i = i*(decimal)10;
					sRa = i.ToString ("0.#####");
				}
			}
			if (bRm) {
				Decimal i = Convert.ToDecimal (sRa);
				if (Math.Abs (i) > (decimal) 0.0001) { 
					i = i/(decimal)10;
					sRa = i.ToString ("0.#####");
				}
			}
			if (bRs) {
				sRa = (-Convert.ToDecimal (sRa)).ToString ();
			}
			if (bRa) {
				ndX = Convert.ToDouble (sRa) + Convert.ToDouble (nsX);
				nsX = ndX.ToString ("0.#####");
			}

			// Normal buttons
			if (bNp) {
				Decimal i = Convert.ToDecimal (sNa);
				if (Math.Abs (i) < (decimal) 1000) { 
					i = i*(decimal)10;
					sNa = i.ToString ("0.#####");
				}
			}
			if (bNm) {
				Decimal i = Convert.ToDecimal (sNa);
				if (Math.Abs (i) > (decimal) 0.0001) { 
					i = i/(decimal)10;
					sNa = i.ToString ("0.#####");
				}
			}
			if (bNs) {
				sNa = (-Convert.ToDecimal (sNa)).ToString ();
			}
			if (bNa) {
				ndY = Convert.ToDouble (sNa) + Convert.ToDouble (nsY);
				nsY = ndY.ToString ("0.#####");
			}

			try {
				ndT = Convert.ToDouble (nsT);
				ndX = Convert.ToDouble (nsX);
				ndY = Convert.ToDouble (nsY);
				ndZ = Convert.ToDouble (nsZ);

			}
			catch (FormatException) {
				//Debug.Log ("Invalid chars, aborting end process");
				return;
			}
			bool changed = false;
			if (ndT != lastActiveNode.UT) {
				lastActiveNode.UT = ndT;
				lastActiveNode.attachedGizmo.UT = ndT;
				changed = true;
			}
			if (ndX != lastActiveNode.DeltaV.x) {
				lastActiveNode.DeltaV.x = ndX;
				lastActiveNode.attachedGizmo.DeltaV.x = ndX;
				changed = true;
			}
			if (ndY != lastActiveNode.DeltaV.y) {
				lastActiveNode.DeltaV.y = ndY;
				lastActiveNode.attachedGizmo.DeltaV.y = ndY;
				changed = true;
			}
			if (ndZ != lastActiveNode.DeltaV.z) {
				lastActiveNode.DeltaV.z = ndZ;
				lastActiveNode.attachedGizmo.DeltaV.z = ndZ;
				changed = true;
			}

			if (changed) {
				//Debug.Log ("Flight plan updated");
				lastActiveNode.solver.UpdateFlightPlan();
			}

			if (bC0) { FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = PatchRendering.RelativityMode.LOCAL_TO_BODIES; }
			if (bC1) { FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = PatchRendering.RelativityMode.LOCAL_AT_SOI_ENTRY_UT; }
			if (bC2) { FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = PatchRendering.RelativityMode.LOCAL_AT_SOI_EXIT_UT; }
			if (bC3) { FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = PatchRendering.RelativityMode.RELATIVE; }
			if (bC4) { FlightGlobals.ActiveVessel.patchedConicRenderer.relativityMode = PatchRendering.RelativityMode.DYNAMIC; }

			//if (bClose) { lastActiveNode = null; }

		}

		private static bool IsEnabled
		{
			get { return FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null; }
		}
	}	
}


