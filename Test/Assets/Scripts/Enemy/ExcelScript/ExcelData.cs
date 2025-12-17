using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class ExcelData : ScriptableObject
{
	public List<EnemyDataEntity> Enemy; // Replace 'EntityType' to an actual enemyType that is serializable.
}
