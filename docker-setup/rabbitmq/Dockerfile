FROM rabbitmq:4.1-management

COPY enabled_plugins /etc/rabbitmq/enabled_plugins
COPY rabbitmq.conf   /etc/rabbitmq/conf.d/10-custom.conf

EXPOSE 5672 15672
