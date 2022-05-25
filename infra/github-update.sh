#!/bin/sh
#This is a simple bash script that will poll github for changes to your repo,
#if found pull them down, and then rebuild and restart a Jekyll site running
#in Nginx. Note, we cannot use cron to schedule a job every 5 seconds, so we create
#a script that executes an infinite loop that sleeps every 5 seconds
#We run the script with nohup so it executes as a background process: $nohup ./update-jekyll

while true
do

cd ~/api-auto-archiver;

git fetch;
LOCAL=$(git rev-parse HEAD);
REMOTE=$(git rev-parse @{u});

#if our local revision id doesn't match the remote, we will need to pull the changes
if [ $LOCAL != $REMOTE ]; then
    echo "pulled new code and merging"
    git pull origin main;

    echo "stopping kestrel"
    sudo systemctl stop kestrel.service

    sudo dotnet publish /home/dave/api-auto-archiver/ --configuration Release --output /var/www

    echo "starting kestrel"
    sudo systemctl start kestrel.service
fi
sleep 5
done