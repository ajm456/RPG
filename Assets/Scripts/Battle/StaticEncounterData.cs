﻿using UnityEngine;
using System.Collections.Generic;

public struct EncounterData
{
	public List<string> heroNames;
	public List<string> enemyNames;

	public EncounterData(List<string> heroNames, List<string> enemyNames)
	{
		this.heroNames = heroNames;
		this.enemyNames = enemyNames;
	}

	/// <summary>
	/// Set the list of enemies for this encounter, using the current party as
	/// the hero-side combatants.
	/// </summary>
	/// <param name="enemyNames">A list of names of the enemies to load.</param>
	public EncounterData(List<string> enemyNames)
	{
		heroNames = JsonParser.GetCurrentPartyNames();
		this.enemyNames = enemyNames;
	}

	public EncounterData(EncounterData data)
	{
		heroNames = data.heroNames;
		enemyNames = data.enemyNames;
	}
};

/*
 * Sets the initial state for a battle. Set this data before transitioning into
 * a battle scene, and read from it afterwards.
 */
public static class EncounterDataStaticContainer
{
	// Raise this flag to indicate that data was set before loading into a
	// battle scene
	private static bool isSet;

	private static EncounterData data;


	public static void SetData(EncounterData data)
	{
		EncounterDataStaticContainer.data = data;
		isSet = true;
	}

	public static EncounterData GetData()
	{
		if(!isSet)
		{
			Debug.Log("Encounter scene data is unset - did you forget to call EncounterDataStaticContainer.SetData() before loading a battle?");
			Debug.Break();
		}

		EncounterData retval = new EncounterData(data);

		// Unset data and lower flag so errors are more obvious
		data.heroNames = null;
		data.enemyNames = null;
		isSet = false;

		return retval;
	}

	/// <summary>
	/// Returns whether or not data has been set for this encounter.
	/// </summary>
	/// <returns></returns>
	public static bool IsDataSet()
	{
		return isSet;
	}
}
