<p align="center">
  <img src="/screenshots/logo.png" width="410" height="250">
</p>

<h4 align="center">Control your <a href="https://www.digitalocean.com" target="_blank">DigitalOcean</a> resources from the Telegram messenger</h4>

<p align="center">
<img src="https://api.codacy.com/project/badge/Grade/44bbed52b81a4718891619b496d13244"/>
<img src="https://github.com/podobaas/DigitalOceanTelegramBot/workflows/DigitalOceanBot%20CI/badge.svg"/>
<img src="https://img.shields.io/github/license/podobaas/DigitalOceanTelegramBot"/>
</p>

<p align="center">
  <a href="https://telegram.me/DigitalOceanDropletBot" target="_blank">Link to Telegram bot</a> |
  <a href="https://www.digitalocean.com/community/tools/digitalocean-for-telegram" target="_blank">DigitalOcean Community Tools Page</a> 
</p>   
  
<p align="center">
    <img src="/screenshots/screen.png" width="400" height="490">
  </a>
</p>

✺ Authorization based on DigitalOcean Application for API

✺ For more security, **your token is auto revoked after 15 minutes** after authorization in DO account!

<br/>

<h4 align="center">This project is supported by:</h4>
<p p align="center">
  <a href="https://www.digitalocean.com/">
    <img src="https://opensource.nyc3.cdn.digitaloceanspaces.com/attribution/assets/SVG/DO_Logo_horizontal_blue.svg" width="201px">
  </a>
</p>

## Basic commands

**/start** - Authentication in DigitalOcean account

**/stop** - Revokation your token and delete session data from database

## Available functional
<details><summary><b>Account</b> </summary>

+ Account information
+ Information about your credit

</details>


<details><summary><b>Droplets</b> </summary>
  
+ Create new droplet
+ Rename droplet
+ Reboot droplet
+ Power cycle droplet
+ Shutdouwn droplet
+ Power on droplet
+ Create snapshot
+ Reset password

</details>


<details><summary><b>Firewalls</b> </summary>
  
+ Create new firewall
+ Add inbound rule to firewall
+ Add outbound rule to firewall
+ Add droplets to firewall
+ Remove droplets from firewall

</details>

<details><summary><b>Projects</b> </summary>
  
+ Create new project
+ Rename project
+ Change description
+ Change purpose
+ Change environment
+ Set as default project

</details>

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

## Built With
+ [.NET Core](https://github.com/dotnet/core)
+ [TelegramBot API .NET](https://github.com/TelegramBots/Telegram.Bot)
+ [DigitalOcean API .NET](https://github.com/trmcnvn/DigitalOcean.API)
+ [MongoDB](https://github.com/mongodb/mongo)
+ [RabbitMQ](https://github.com/rabbitmq/rabbitmq-server)
+ [EasyNetQ](https://github.com/EasyNetQ/EasyNetQ)
+ [Serilog](https://github.com/serilog/serilog)
+ [NSubstitute](https://github.com/nsubstitute/NSubstitute)

## References
+ [LICENSE](LICENSE)
+ [CHANGELOG](CHANGELOG.MD)
+ [CONTRIBUTING](CONTRIBUTING.md)
