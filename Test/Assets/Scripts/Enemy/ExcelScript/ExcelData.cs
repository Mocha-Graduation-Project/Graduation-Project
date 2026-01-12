using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExcelAsset]
public class ExcelData : ScriptableObject
{
	public List<EnemyDataEntity> Enemy;
	public List<MoveBossDataEntity> MoveBoss;
	public List<DepthBossDataEntity> DepthBoss;
}
