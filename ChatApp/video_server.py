import socket
import threading
import json
import time
from datetime import datetime
import sys

# để 0.0.0.0 để máy khác trong mạng LAN/Internet kết nối được
TCP_HOST = '0.0.0.0'
TCP_PORT = 8000
UDP_HOST = '0.0.0.0'
UDP_PORT = 8001

class VideoServer:
    def __init__(self):
        self.tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.tcp_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.tcp_socket.bind((TCP_HOST, TCP_PORT))
        
        self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.udp_socket.bind((UDP_HOST, UDP_PORT))
        
        self.online_clients = {}
        self.active_calls = {}
        self.call_counter = 0
        self.lock_clients = threading.Lock()
        self.lock_calls = threading.Lock()

    def start(self):
        
        print(f"✅ VIDEO SERVER STARTED | Listening on {TCP_HOST}:{TCP_PORT}")
        print(f"ℹ️  LAN/Online Mode Ready. Make sure Firewall is OFF or Ports {TCP_PORT}/{UDP_PORT} are OPEN.")
        
        threading.Thread(target=self.handle_tcp_signaling, daemon=True).start()
        threading.Thread(target=self.handle_udp_video, daemon=True).start()
        try:
            while True: time.sleep(1)
        except KeyboardInterrupt:
            self.tcp_socket.close()
            self.udp_socket.close()

    def handle_tcp_signaling(self):
        self.tcp_socket.listen(10)
        while True:
            try:
                client_socket, address = self.tcp_socket.accept()
                threading.Thread(target=self.handle_client_signaling, args=(client_socket,address), daemon=True).start()
            except: break

    def handle_client_signaling(self, client_socket, address):
        username = None
        buffer = ""
        try:
            while True:
                data = client_socket.recv(4096).decode('utf-8')
                if not data: break
                buffer += data
                while "\n" in buffer:
                    line, buffer = buffer.split("\n",1)
                    try:
                        message = json.loads(line)
                        action = message.get('action')
                        
                        if action == 'login':
                            username = message.get('username')
                            udp_port = message.get('udp_port')
                            with self.lock_clients:
                                if username in self.online_clients:
                                    client_socket.sendall((json.dumps({'status':'error','message':'Username taken'})+"\n").encode('utf-8'))
                                else:
                                    # Lưu IP thật của client để gửi UDP về đúng chỗ
                                    self.online_clients[username] = {'tcp_socket':client_socket,'udp_address':(address[0],udp_port),'in_call':False,'call_with':None}
                                    print(f"--> [LOGIN] {username} from {address[0]}")
                                    client_socket.sendall((json.dumps({'status':'ok'})+"\n").encode('utf-8'))

                        elif action == 'call':
                            caller = message.get('from')
                            callee = message.get('to')
                            with self.lock_clients:
                                if callee in self.online_clients and not self.online_clients[callee]['in_call']:
                                    with self.lock_calls:
                                        self.call_counter += 1
                                        call_id = f"call_{self.call_counter}"
                                        self.active_calls[call_id] = {'participants':[caller,callee]}
                                    
                                    self.online_clients[callee]['tcp_socket'].sendall((json.dumps({'action':'incoming_call','from':caller,'call_id':call_id})+"\n").encode('utf-8'))
                                    client_socket.sendall((json.dumps({'status':'ok','call_id':call_id})+"\n").encode('utf-8'))
                                    print(f"--> [CALL] {caller} calling {callee}")

                        elif action == 'accept_call':
                            call_id = message.get('call_id')
                            user = message.get('username')
                            with self.lock_calls:
                                if call_id in self.active_calls:
                                    parts = self.active_calls[call_id]['participants']
                                    other = parts[0] if parts[1] == user else parts[1]
                                    with self.lock_clients:
                                        self.online_clients[user]['in_call'] = True; self.online_clients[user]['call_with'] = other
                                        self.online_clients[other]['in_call'] = True; self.online_clients[other]['call_with'] = user
                                        
                                        msg = (json.dumps({'action':'call_accepted','call_id':call_id,'with':user})+"\n").encode('utf-8')
                                        self.online_clients[other]['tcp_socket'].sendall(msg)
                                        print(f"--> [ACCEPT] Call {call_id} started")

                        elif action == 'end_call': # Xử lý đơn giản cho LAN
                             call_id = message.get('call_id')
                             if call_id: self.end_call_logic(call_id)

                    except: pass
        except: pass
        finally:
            if username and username in self.online_clients:
                del self.online_clients[username]
            client_socket.close()

    def end_call_logic(self, call_id):
        with self.lock_calls:
            if call_id in self.active_calls:
                parts = self.active_calls[call_id]['participants']
                for p in parts:
                    if p in self.online_clients:
                        try:
                            self.online_clients[p]['tcp_socket'].sendall((json.dumps({'action':'call_ended'})+"\n").encode('utf-8'))
                            self.online_clients[p]['in_call'] = False
                        except: pass
                del self.active_calls[call_id]

    def handle_udp_video(self):
        while True:
            try:
                data, addr = self.udp_socket.recvfrom(65535)
                # Parse Header: username:call_id||data
                header_end = data.find(b'||')
                if header_end != -1:
                    header = data[:header_end].decode('utf-8').split(':')
                    if len(header) >= 2:
                        sender, call_id = header[0], header[1]
                        if call_id in self.active_calls:
                            parts = self.active_calls[call_id]['participants']
                            recipient = parts[0] if parts[1] == sender else parts[1]
                            if recipient in self.online_clients:
                                # Forward gói tin sang địa chỉ UDP của người nhận
                                self.udp_socket.sendto(data, self.online_clients[recipient]['udp_address'])
            except: pass

if __name__ == '__main__':
    server = VideoServer()
    server.start()