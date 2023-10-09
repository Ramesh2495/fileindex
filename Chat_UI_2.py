import tkinter as tk
from tkinter import scrolledtext
import paho.mqtt.client as mqtt

root = tk.Tk()
root.title("MQTT Chat Application")

valid_username = "mobilni2"
valid_password = "Systemy"

broker_address = "pcfeib425t.vsb.cz" 
port = 1883

mqtt_client = mqtt.Client("ChatClient")

def login():
    entered_username = username_entry.get()
    entered_password = password_entry.get()

    if (
        entered_username == valid_username
        and entered_password == valid_password
    ):
        login_frame.grid_forget() 
        chat_frame.grid(row=0, column=0, padx=10, pady=10, columnspan=2)
        mqtt_connect(entered_username) 
        mqtt_set_status("online") 

def mqtt_connect(username):
    mqtt_client.username_pw_set(username, valid_password)
    mqtt_client.connect(broker_address, port)
    mqtt_client.subscribe("/mschat/all/#")  
    mqtt_client.subscribe(f"/mschat/user/{username}/#")  
    mqtt_client.on_message = on_message
    mqtt_client.will_set(f"/mschat/status/{username}", payload="offline", qos=0, retain=True) 
    mqtt_client.loop_start()

def send_private_message():
    recipient = recipient_entry.get()
    message = message_entry.get()
    if recipient and message:
        mqtt_client.publish(f"/mschat/user/{recipient}/{valid_username}", message)
        chat_window.config(state="normal")
        chat_window.insert(tk.END, f"You to {recipient}: {message}\n")
        chat_window.config(state="disabled")
        recipient_entry.delete(0, tk.END)
        message_entry.delete(0, tk.END)

def on_message(client, userdata, message):
    message_text = message.payload.decode()
    if message.topic.startswith("/mschat/user/"):
        sender = message.topic.split("/")[-1]
        chat_window.config(state="normal")
        chat_window.insert(tk.END, f"{sender}: {message_text}\n")
        chat_window.config(state="disabled")

def mqtt_set_status(status):
    mqtt_client.publish(f"/mschat/status/{valid_username}", status, retain=True)

login_frame = tk.Frame(root)
login_frame.grid(row=0, column=0, padx=10, pady=10)

username_label = tk.Label(login_frame, text="Username:")
username_label.grid(row=0, column=0, padx=10, pady=5)
username_entry = tk.Entry(login_frame)
username_entry.grid(row=0, column=1, padx=10, pady=5)

password_label = tk.Label(login_frame, text="Password:")
password_label.grid(row=1, column=0, padx=10, pady=5)
password_entry = tk.Entry(login_frame, show="*")
password_entry.grid(row=1, column=1, padx=10, pady=5)

login_button = tk.Button(login_frame, text="Login", command=login)
login_button.grid(row=2, columnspan=2, padx=10, pady=10)

chat_frame = tk.Frame(root)
chat_window = scrolledtext.ScrolledText(chat_frame, width=40, height=10, state="disabled")
chat_window.grid(row=0, column=0, padx=10, pady=10, columnspan=2)

# Recipient entry field for private messages
recipient_label = tk.Label(chat_frame, text="Recipient:")
recipient_label.grid(row=1, column=0, padx=10, pady=10)
recipient_entry = tk.Entry(chat_frame, width=20)
recipient_entry.grid(row=1, column=1, padx=10, pady=10)

# Message entry field
message_entry = tk.Entry(chat_frame, width=30)
message_entry.grid(row=2, column=0, padx=10, pady=10)

# Send private message button
send_private_button = tk.Button(chat_frame, text="Send Private", command=send_private_message)
send_private_button.grid(row=2, column=1, padx=10, pady=10)

# Initially, show only the login frame
chat_frame.grid_forget()

# Start the Tkinter main loop
root.mainloop()
