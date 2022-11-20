FROM localstack/localstack:1.2.0

ENV TF_VERSION=1.3.5

RUN apt-get update && apt-get install wget

RUN wget https://releases.hashicorp.com/terraform/${TF_VERSION}/terraform_${TF_VERSION}_linux_amd64.zip && \
    unzip terraform_${TF_VERSION}_linux_amd64.zip && \
    mv terraform /usr/local/bin/terraform && \
    chmod +x /usr/local/bin/terraform && \
    rm terraform_${TF_VERSION}_linux_amd64.zip

COPY ./setup/ /etc/localstack/init/ready.d/

RUN chmod -R 744 /etc/localstack/init/ready.d/

ENTRYPOINT ["docker-entrypoint.sh"]