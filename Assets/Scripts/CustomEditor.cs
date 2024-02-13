using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Main)), CanEditMultipleObjects]
public class PropertyHolderEditor : Editor {

	public SerializedProperty
		agentMaxSpeed_Prop,
		agentPrefabs_Prop,
		agentPrefabGroup_Prop,
		alpha_Prop,
		avoidanceRadius_Prop,
		circleRadius_Prop,
		gridPrefab_Prop,
		groupAgentPrefabs_Prop,
		handleCollison_Prop,
		lcpsolver_Prop,
		lcpsolverEpsilon_Prop,
		mapGen_Prop,
		neighbourBins_Prop,
		numberOfAgents_Prop,
		numberOfCells_Prop,
		numberOfDiscRows_Prop,
		percentOfGroupedAgents_Prop,
		percentTwo_Prop,
		percentThree_Prop,
		percentFour_Prop,
		planePrefab_Prop,
		planeSize_Prop,
		roadNode_Prop,
		rowAmount_Prop,
		rowLength_Prop,
		shirtColorPrefab_Prop,
		skip_Prop,
		solverIterations_Prop,
		smoothTurns_Prop,
		spawnerPrefab_Prop,
		spawnMethod_Prop,
		timeStep_Prop,
		useSimpleAgents_Prop,
		usePresetGroupDistances_Prop,
		visualizeDensity_Prop,
		visualizeVelocity_Prop,
		visibleMap_Prop,
		walkBack_Prop;
		

		

	
	void OnEnable () {
		// Setup the SerializedProperties
		agentMaxSpeed_Prop = serializedObject.FindProperty ("agentMaxSpeed");
		agentPrefabs_Prop = serializedObject.FindProperty ("agentPrefabs");
		agentPrefabGroup_Prop = serializedObject.FindProperty("agentPrefabGroup");
		alpha_Prop = serializedObject.FindProperty ("alpha");
		avoidanceRadius_Prop = serializedObject.FindProperty ("agentAvoidanceRadius");
		circleRadius_Prop = serializedObject.FindProperty ("circleRadius");
		gridPrefab_Prop = serializedObject.FindProperty ("gridPrefab");
		groupAgentPrefabs_Prop = serializedObject.FindProperty ("groupAgentPrefabs");
		handleCollison_Prop = serializedObject.FindProperty ("handleCollision");
		lcpsolver_Prop = serializedObject.FindProperty ("solver");
		lcpsolverEpsilon_Prop = serializedObject.FindProperty ("epsilon");
		mapGen_Prop = serializedObject.FindProperty ("mapGen");
		neighbourBins_Prop = serializedObject.FindProperty ("neighbourBins");
		numberOfAgents_Prop = serializedObject.FindProperty ("numberOfAgents");
		numberOfCells_Prop = serializedObject.FindProperty ("cellsPerRow");
		numberOfDiscRows_Prop = serializedObject.FindProperty ("numberOfDiscRows");
		percentOfGroupedAgents_Prop = serializedObject.FindProperty ("percentOfGroupedAgents");
		percentTwo_Prop = serializedObject.FindProperty ("percentOfTwoInGroup");
		percentThree_Prop = serializedObject.FindProperty ("percentOfThreeInGroup");
		percentFour_Prop = serializedObject.FindProperty ("percentOfFourInGroup");
		planePrefab_Prop = serializedObject.FindProperty ("plane");
		planeSize_Prop = serializedObject.FindProperty ("planeSize");
		roadNode_Prop = serializedObject.FindProperty ("roadNodeAmount");
		rowAmount_Prop = serializedObject.FindProperty ("rows");
		rowLength_Prop = serializedObject.FindProperty ("rowLength");
		shirtColorPrefab_Prop = serializedObject.FindProperty ("shirtColorPrefab");
		solverIterations_Prop = serializedObject.FindProperty ("solverMaxIterations");
		skip_Prop = serializedObject.FindProperty ("skipNodeIfSeeNext");
		smoothTurns_Prop = serializedObject.FindProperty ("smoothTurns");
		spawnerPrefab_Prop = serializedObject.FindProperty ("spawnerPrefab");
		spawnMethod_Prop = serializedObject.FindProperty ("spawnMethod");
		timeStep_Prop = serializedObject.FindProperty ("timeStep");
		useSimpleAgents_Prop = serializedObject.FindProperty ("useSimpleAgents");
		usePresetGroupDistances_Prop = serializedObject.FindProperty ("usePresetGroupDistances");
		visualizeDensity_Prop = serializedObject.FindProperty ("showSplattedDensity");
		visualizeVelocity_Prop = serializedObject.FindProperty ("showSplattedVelocity");
		visibleMap_Prop = serializedObject.FindProperty ("visibleMap");
		walkBack_Prop = serializedObject.FindProperty ("walkBack");
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update ();
		EditorGUILayout.PropertyField(planeSize_Prop);
		EditorGUILayout.PropertyField(roadNode_Prop);
		EditorGUILayout.PropertyField(numberOfCells_Prop);
		EditorGUILayout.PropertyField (neighbourBins_Prop);
		EditorGUILayout.PropertyField(agentMaxSpeed_Prop);
		EditorGUILayout.PropertyField(timeStep_Prop);
		EditorGUILayout.PropertyField(alpha_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(spawnMethod_Prop);

		Main.Method m = (Main.Method)spawnMethod_Prop.enumValueIndex;
		switch( m ) {
		case Main.Method.uniformSpawn:
			EditorGUILayout.PropertyField (numberOfAgents_Prop);
			break;
		case Main.Method.circleSpawn:        
			EditorGUILayout.PropertyField (circleRadius_Prop);   
			EditorGUILayout.IntSlider (numberOfAgents_Prop, 0, (int)(2*Mathf.PI*circleRadius_Prop.floatValue/(avoidanceRadius_Prop.floatValue*2f)));
			break;
			
		case Main.Method.discSpawn:  
			EditorGUILayout.PropertyField(circleRadius_Prop); 
			EditorGUILayout.IntSlider (numberOfDiscRows_Prop, 0, (int)((planeSize_Prop.floatValue*5-circleRadius_Prop.floatValue)/(avoidanceRadius_Prop.floatValue*2f)));
			break;

		case Main.Method.continuousSpawn:     
			EditorGUILayout.PropertyField (numberOfAgents_Prop);
			EditorGUILayout.PropertyField (percentOfGroupedAgents_Prop);
			if (percentOfGroupedAgents_Prop.floatValue > 0.0f) {
				float diff1 = 1.0f - percentThree_Prop.floatValue; diff1 = diff1 < 0 ? 0 : diff1;
				float diff2 = 1.0f - percentTwo_Prop.floatValue - percentThree_Prop.floatValue; diff2 = diff2 < 0.01 ? 0 : diff2; 
				float diff3 = 1.0f - percentTwo_Prop.floatValue; diff3 = diff1 < 0 ? 0 : diff3;
				EditorGUILayout.Slider (percentTwo_Prop, 0.0f, diff1);
				EditorGUILayout.Slider (percentThree_Prop, 0.0f, diff3);
				percentFour_Prop.floatValue = diff2; 
				EditorGUILayout.PropertyField (percentFour_Prop);
			}
			EditorGUILayout.PropertyField (useSimpleAgents_Prop);
			break;

		case Main.Method.areaSpawn:
			EditorGUILayout.PropertyField (rowAmount_Prop);
			EditorGUILayout.PropertyField (rowLength_Prop);
			break;
		}

		EditorGUILayout.PropertyField(lcpsolver_Prop);
		EditorGUILayout.PropertyField (solverIterations_Prop);
		EditorGUILayout.PropertyField (lcpsolverEpsilon_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(visualizeDensity_Prop);
		EditorGUILayout.PropertyField(visualizeVelocity_Prop);
		EditorGUILayout.PropertyField(visibleMap_Prop);
		EditorGUILayout.PropertyField(walkBack_Prop);
		EditorGUILayout.PropertyField(skip_Prop);
		EditorGUILayout.PropertyField (smoothTurns_Prop);
		EditorGUILayout.PropertyField (handleCollison_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(avoidanceRadius_Prop);
		EditorGUILayout.PropertyField(usePresetGroupDistances_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField (agentPrefabs_Prop);
		EditorGUILayout.PropertyField (groupAgentPrefabs_Prop);
		EditorGUILayout.PropertyField(shirtColorPrefab_Prop);
		EditorGUILayout.PropertyField(gridPrefab_Prop);
		EditorGUILayout.PropertyField(mapGen_Prop);
		EditorGUILayout.PropertyField(planePrefab_Prop);
		EditorGUILayout.PropertyField(spawnerPrefab_Prop);


		serializedObject.ApplyModifiedProperties ();
	}
}