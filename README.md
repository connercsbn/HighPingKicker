# High Ping Kicker
Inspired by SourceMod plugins [Very Basic High Ping Kicker](https://forums.alliedmods.net/showthread.php?p=769939) and [High Ping Kicker](https://github.com/ZK-Servidores/High-Ping-Kicker)

### Features
 - Admin immunity (anyone with any flag or group is immune)
 - Grace period on map change and when first connected


### Config
When the plugin is first loaded, the following config will be generated in `counterstrikesharp/configs/plugins/HighPingKicker/HighPingKicker.json`

```js
{
   "max_ping": 150,
   "max_warnings": 5,
   "check_interval": 20,  // in seconds, can be decimal number
   "show_warnings": true,
   "show_public_kick_message": true,
   "warning_message": "You will be kicked for excessive ping. You have {WARN} out of {MAXWARN} warnings.",
   "kick_message": "{NAME} has been kicked due to excessive ping.",
   "grace_period_seconds": 90,
   "dev": false  // enables additional logging
   "ConfigVersion": 2,
}
```

### Configurable messages

| Message type     | Broadcast to         |    Default                                                                            |
| ---------------- | ------------         | -------------                                                                         | 
| Warning message  | The player being warned  | You will be kicked for excessive ping. You have {WARN} out of {MAXWARN} warnings.     | 
| Kick message     | Everyone             | {NAME} has been kicked due to excessive ping.                                         |

#### Available message variables
 - {NAME}
 - {WARN}
 - {MAXWARN}
 - {PING}
 - {MAXPING}
