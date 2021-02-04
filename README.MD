<p align="center">
  <img src="/screenshots/logo.png" width="410" height="250">
</p>

<h4 align="center">Control your <a href="https://www.digitalocean.com" target="_blank">DigitalOcean</a> resources from the Telegram messenger</h4>

<p align="center">
<img alt="Docker Pulls" src="https://img.shields.io/docker/pulls/podobaas/digitalocean-telegram-bot">
<img alt="GitHub last commit (branch)" src="https://img.shields.io/github/last-commit/podobaas/DigitalOceanTelegramBot/master">
<img src="https://img.shields.io/github/license/podobaas/DigitalOceanTelegramBot"/>
</p>

<p align="center">
  <a href="https://hub.docker.com/r/podobaas/digitalocean-telegram-bot" target="_blank">DockerHub Page</a> 
|
  <a href="https://www.digitalocean.com/community/tools/digitalocean-for-telegram" target="_blank">DigitalOcean Community Tools Page</a> 
</p>   
  
<p align="center">
    <img src="/screenshots/screen.png" width="400" height="490">
  </a>
</p>

<br/>

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

## Usage
- Create your personal bot. Just talk to [@BotFather](https://t.me/botfather) or read [manual](https://core.telegram.org/bots#3-how-do-i-create-a-bot)
- Get your UserId in Telegram [@getmyid_bot](https://t.me/getmyid_bot)
- Create a personal [DigitalOcean token](https://cloud.digitalocean.com/account/api/tokens)
- Pull docker image ``docker pull podobaas/digitalocean-telegram-bot``
- Run ``docker run -d --env TELEGRAM_BOT_TOKEN=XXX --env TELEGRAM_USER_ID=XXX --env DIGITALOCEAN_TOKEN=XXX --name my_do_bot podobaas/digitalocean-telegram-bot:1.0``
- Enjoy!

## Built With
+ [.NET 5](https://github.com/dotnet/core)
+ [TelegramBot API .NET](https://github.com/TelegramBots/Telegram.Bot)
+ [DigitalOcean API .NET](https://github.com/trmcnvn/DigitalOcean.API)
+ [Scrutor](https://github.com/khellang/Scrutor)

## References
+ [LICENSE](LICENSE)
+ [CHANGELOG](CHANGELOG.MD)
+ [CONTRIBUTING](CONTRIBUTING.md)
