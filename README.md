# High Ping Kicker
Inspired by SourceMod plugins [Very Basic High Ping Kicker](https://forums.alliedmods.net/showthread.php?p=769939) and [High Ping Kicker](https://github.com/ZK-Servidores/High-Ping-Kicker)

### Features
 - Admin immunity (anyone with any flag or group is immune)
 - Grace period on map change and when first connected


### Config
When the plugin is first loaded, the following config will be generated in `counterstrikesharp/configs/plugins/HighPingKicker/HighPingKicker.json`

<pre>
{
   "max_ping": 150,
   "max_warnings": 5,
   "check_interval": 20,  // in seconds, can be decimal number
   "show_warnings": true,
   "show_public_kick_message": true,
   "warning_message": "You will be kicked for excessive ping. You have {WARN} out of {MAXWARN} warnings.",
   "kick_message": "{NAME} has been kicked due to excessive ping.",
   "grace_period_seconds": 90,
   "dev": false
   "ConfigVersion": 2,
}
</pre>

### Configurable messages

| Message  | Default | Variables you can use |
| ------------- | ------------- | ------- |
| Warning message  | You will be kicked for excessive ping. You have {WARN} out of {MAXWARN} warnings.  | {WARN}, {MAXWARN}, {PING}
| Kick message  | {NAME} has been kicked due to excessive ping.  | {WARN}, {MAXWARN}, {PING}, {NAME}
