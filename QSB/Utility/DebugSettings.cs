﻿using Newtonsoft.Json;

namespace QSB.Utility;

[JsonObject(MemberSerialization.OptIn)]
public class DebugSettings
{
	[JsonProperty("useKcpTransport")]
	private bool _useKcpTransport;
	public bool UseKcpTransport => _useKcpTransport || AutoStart;

	[JsonProperty("dumpWorldObjects")]
	public bool DumpWorldObjects;

	[JsonProperty("instanceIdInLogs")]
	public bool InstanceIdInLogs;

	[JsonProperty("hookDebugLogs")]
	public bool HookDebugLogs;

	[JsonProperty("avoidTimeSync")]
	public bool AvoidTimeSync;

	[JsonProperty("autoStart")]
	public bool AutoStart;

	[JsonProperty("debugMode")]
	public bool DebugMode;

	[JsonProperty("drawGui")]
	private bool _drawGui;
	public bool DrawGui => DebugMode && _drawGui;

	[JsonProperty("drawLines")]
	private bool _drawLines;
	public bool DrawLines => DebugMode && _drawLines;

	[JsonProperty("drawLabels")]
	private bool _drawLabels;
	public bool DrawLabels => DebugMode && _drawLabels;

	[JsonProperty("drawQuantumVisibilityObjects")]
	private bool _drawQuantumVisibilityObjects;
	public bool DrawQuantumVisibilityObjects => DebugMode && _drawQuantumVisibilityObjects;

	[JsonProperty("drawGhostAI")]
	private bool _drawGhostAI;
	public bool DrawGhostAI => DebugMode && _drawGhostAI;

	[JsonProperty("skipTitleScreen")]
	private bool _skipTitleScreen;
	public bool SkipTitleScreen => DebugMode && _skipTitleScreen;

	[JsonProperty("greySkybox")]
	private bool _greySkybox;
	public bool GreySkybox => DebugMode && _greySkybox;
}
