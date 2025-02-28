# BirthdayTrackBot

## About
This telegram bot reminds you and helps you to track your friends' birthdays 😇

## How to launch (via Docker)
0. Download/Launch [Docker desktop](https://www.docker.com/)
1. `docker pull mos1ck/birthday-bot:1.2` in cmd/terminal    
2. Download *docker-compose.yaml* from the rep
3. Create your MySql db 
4. Get yourself a tg Bot Token ( It'll only take a few minutes, search for @BotFather in tg)
5. ( Alt to 6.) Download *.env.example* in the same directory, rename it to *.env* and add your credentials there
6. ( Alt to 5.) Or just replace all \${...} with your db credentials and token in *docker-compose.yaml* (replace ${DOCKER_HUB_USER} with mos1ck)
7. `docker-compose up -d` in cmd/terminal

Your bot should be up-and-running! 😏
(In theory, you might be able to see the bot working without db)

## Functionality
You register in a system to see your friends' birthdays.
People can register by getting inviatations.
To be the first one to register, enter last 7 characters of Bot Token