#!/bin/sh

# use the auto-archiver server build for main build, then run dotnet install here

# currently using filezilla for transferring auto-archiver secrets

# rsync -e 'ssh -p 31' --rsync-path="sudo rsync" secrets/kestrel.service  dave@pfsense:/etc/systemd/system/kestrel.service

# ssh pfsense -p 30 

# git clone https://github.com/djhmateer/api-security-test ; sudo chmod +x ~/api-security-test/infra/server-build.sh ; ./api-security-test/infra/server-build.sh

sudo apt update -y
sudo apt upgrade -y

# **Hatespeech**
# copied from osr4rightstools 2hatespeechinfracreateimage

# sudo apt install python3-pip -y
# export PATH=/home/dave/.local/bin:$PATH

# pip3 install numpy
# pip3 install nltk
# pip3 install pandas
# pip3 install keras
# pip3 install testresources
# pip3 install tensorflow
# pip3 install scikit-learn
# pip3 install preprocessor
# pip3 install textblob
# pip3 install transformers

# pip3 install sentencepiece

# # warnings but I think okay
# python3 -m nltk.downloader stopwords
# python3 -m nltk.downloader punkt

# python3 -m textblob.download_corpora

# sudo apt install unzip -y

# # Hatespeech source
# sudo git clone https://github.com/khered20/Prepared_HateSpeech_Models /home/dave/hatespeech

# sudo chown -R dave:dave /home/dave/hatespeech 
# sudo chmod +x /home/dave/hatespeech

# # # Prepared model
# cd /home/dave/hatespeech
# wget https://functionsdm2storage.blob.core.windows.net/outputfiles/_all_train_results-20210712T152424Z-001.zip

# unzip _all_train_results-20210712T152424Z-001.zip -d /home/dave/hatespeech


# **API**

# Install packages for .NET for Ubutu 20.04 LTS
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

## Install .NET SDK  as we're doing a build
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-6.0


# compile and publish the api
sudo dotnet publish /home/dave/api-security-test/ --configuration Release --output /var/www

# change ownership of the published files 
sudo chown -R www-data:www-data /var/www
# allow exective permissions
sudo chmod +x /var/www

# for test this is a useful script to keep up to date with GH
sudo chmod +x /home/dave/api-security-test/infra/github-update.sh

# auto start on machine reboot
sudo systemctl enable kestrel.service

# logs are in /var/log/syslog
# look for dotnet-api


# rename the machine
sudo hostnamectl set-hostname api-security-test

sudo reboot now

# NOTES

# need nopasswd enabled (which it is on this image)
# sudo visudo
# dave ALL=(ALL) NOPASSWD: ALL
# https://superuser.com/questions/138893/scp-to-remote-server-with-sudo
# copy secrets onto machine

