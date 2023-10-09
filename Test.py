import paho.mqtt.client as mqtt
import time

# MQTT broker settings
broker_address = "pcfeib425t.vsb.cz"
port = 1883

client_id = "Client1"

chat_topic = "chat"

message_cache = {}

def on_message(client, userdata, message):
    timestamp = time.strftime("%Y-%m-%d %H:%M:%S")
    message_cache[timestamp] = message.payload.decode()
    print(f"Received message '{message.payload.decode()}' on topic '{message.topic}' at {timestamp}")

client = mqtt.Client(client_id)

client.on_message = on_message

client.connect(broker_address, port)

client.subscribe(chat_topic)

client.loop_start()

while True:
    message = input("Enter a message (or 'exit' to quit): ")
    if message.lower() == 'exit':
        break
    client.publish(chat_topic, message)

client.disconnect()
client.loop_stop()
