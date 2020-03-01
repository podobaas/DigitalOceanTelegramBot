<p align="center">
  <img src="https://emojis.wiki/emoji-pics/messenger/water-wave-messenger.png" width="100" height="100">
</p>
<h1 align="center">
DigitalOcean for Telegram
</h1>
<p align="center">
<img src="https://api.codacy.com/project/badge/Grade/44bbed52b81a4718891619b496d13244"/>
<img src="https://github.com/podobaas/DigitalOceanTelegramBot/workflows/DigitalOceanBot%20CI/badge.svg"/>
<img src="https://img.shields.io/github/license/podobaas/DigitalOceanTelegramBot"/>
</p>

Control your [DigitalOcean](https://www.digitalocean.com/) resources from the Telegram messenger   <img src="https://i.ibb.co/b5mrrsZ/telegram-1.png" style="vertical-align: middle;" height="22" width="22"></a>

Authorization based on DigitalOcean Application for API

For more security, **your token is auto revoked after 15 minutes** after authorization in DO account!

:link: Telegram link: [DigitalOceanBot](https://t.me/DigitalOceanDropletBot)

## Basic commands

**/start** - Authentication in DigitalOcean account

**/stop** - Revokation your token and delete session data from database

## Available functional:
**Account** :ocean:
+ Account information
+ Information about your credit

**Droplets** :droplet:
+ Create new droplet
+ Rename droplet
+ Reboot droplet
+ Power cycle droplet
+ Shutdouwn droplet
+ Power on droplet
+ Create snapshot
+ Reset password

**Firewalls** :european_castle:
+ Create new firewall
+ Add inbound rule to firewall
+ Add outbound rule to firewall
+ Add droplets to firewall
+ Remove droplets from firewall

**Projects** :pencil2:
+ Create new project
+ Rename project
+ Change description
+ Change purpose
+ Change environment
+ Set as default project

## Missing functional (in development)
+ Block Storage
+ CDN Endpoints
+ Certificates
+ Databases
+ Domains
+ Domain Records
+ Floating IPs
+ Snapshots
+ Kubernetes
+ Load Balancers

## Built With :heart:
+ [TelegramBot API .NET](https://github.com/TelegramBots/Telegram.Bot)
+ [DigitalOcean API .NET](https://github.com/trmcnvn/DigitalOcean.API)
+ [MongoDB](https://github.com/mongodb/mongo)
+ [RabbitMQ](https://github.com/rabbitmq/rabbitmq-server)
+ [EasyNetQ](https://github.com/EasyNetQ/EasyNetQ)
+ [Serilog](https://github.com/serilog/serilog)
+ [NSubstitute](https://github.com/nsubstitute/NSubstitute)

## Screenshots
<img src="/screenshots/1.png" width="250" height="200"> <img src="/screenshots/2.png" width="250" height="200"> <img src="/screenshots/3.png" width="250" height="200">

## References
+ [LICENSE](LICENSE)
+ [CHANGELOG](CHANGELOG.MD)
+ [CONTRIBUTING](CONTRIBUTING.md)
