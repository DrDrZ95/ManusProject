# GitHub SSH Key Setup Instructions

This document provides instructions for adding the generated SSH key to your GitHub account to enable secure code uploads.

## SSH Key Information

- **Key Type**: RSA
- **Key Size**: 4096 bits
- **Key Comment**: github-deploy-key
- **Private Key Location**: `~/.ssh/github_rsa`
- **Public Key Location**: `~/.ssh/github_rsa.pub`

## Public Key

```
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAACAQCHmkIjLJuDOBBUQ7CquOEHyXNreC6CczoLnQ++aGq5BaT01mxto/dCfcbuQyhb3Ip49Cb3jTV1KQipoTvzRrhHupv7TVe2WFNfROdAkUynm6x3/Cj0ZYOe/NWn02a7070zGl7ZDJ20XUGp43GhTHYi7ZvQczjrmPQLJmI3NU02pEbg7/hT+Zucdnoldyxg1G7FtCqmWxvEbN+IwjdQ43zGizgFgc2KLXDYz74RuSB8afKeR11edd2trSyY98z0aOvVriA5qxSZvJPyFbuLQOO1E4DSAvTiWaVXENk8/M0s9MvTU7h12EtkhB0sl6xPSKZnfhP1aMMQV4Pw9Y6kgQBM39b3HWtMilY9surKeHBDphnWKKJmx3/ujag+n6csb6jXNjtJTPjDwJT1pfjLp6UO4LlquGM/WKTz+7853oyMRb9PaQetfi+ufgBOKNMdK91w7/o2J7nQi+MP/IwvGL/3Wm3UelPpY36tbnzRzleTht715cXQtnUNDPJPoEZWeVxWHmRiAyn1Nlv5kEZ3PGnC5QuXkEkjSz2ZbZDPzmv8VNtLgmI4//Svu1teTuf+tsk2hK41eItRk2Czruxf9uW+ew0FCU+rDif1B0rVRsnextQZ8zCv0PdB8BRYS18xV3+zzn4lM4tXb6J3TMa66R7T2bwLom3DqpPbIl0Vs/3Xfw== github-deploy-key
```

## Adding SSH Key to GitHub

1. Log in to your GitHub account
2. Click on your profile picture in the top-right corner and select "Settings"
3. In the left sidebar, click on "SSH and GPG keys"
4. Click the "New SSH key" button
5. Enter a descriptive title for the key (e.g., "DeepSeek Agent Deploy Key")
6. Paste the public key (shown above) into the "Key" field
7. Click "Add SSH key" to save

## Testing the SSH Connection

After adding the key to GitHub, you can test the connection with:

```bash
ssh -T git@github.com -i ~/.ssh/github_rsa
```

You should see a message like: "Hi username! You've successfully authenticated, but GitHub does not provide shell access."

## Using the SSH Key for Git Operations

To use this specific SSH key for Git operations, you can:

1. Create or edit `~/.ssh/config` file with:
   ```
   Host github.com
     IdentityFile ~/.ssh/github_rsa
     User git
   ```

2. Or specify the key directly when cloning:
   ```bash
   GIT_SSH_COMMAND="ssh -i ~/.ssh/github_rsa" git clone git@github.com:username/repository.git
   ```

3. For existing repositories, configure the remote URL to use SSH:
   ```bash
   git remote set-url origin git@github.com:username/repository.git
   ```
