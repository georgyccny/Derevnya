import socket
from transformers import GPT2LMHeadModel, GPT2Tokenizer


model_name = "gpt2"
model = GPT2LMHeadModel.from_pretrained(model_name)
tokenizer = GPT2Tokenizer.from_pretrained(model_name)

def generate_response(prompt):
    input_ids = tokenizer.encode(prompt, return_tensors='pt')
    output = model.generate(input_ids, max_length=100, num_return_sequences=1, no_repeat_ngram_size=2)
    return tokenizer.decode(output[0], skip_special_tokens=True)

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('localhost', 5555))
server_socket.listen(1)

print("Waiting for Unity connection...")
connection, address = server_socket.accept()
print("Connected to Unity")

def recv_until_newline(sock):
    """Receive data from the socket until a newline character is detected."""
    buffer = ''
    while True:
        data = sock.recv(1024).decode('utf-8')
        if not data:
            break
        buffer += data
        if '\n' in buffer:
            break
    return buffer.strip()

while True:
    data = recv_until_newline(connection)
    if not data:
        break
    
    response = generate_response(data)
    print(response)
    # Send responseresponse with a newline delimiter
    connection.sendall((response + '\n').encode('utf-8'))

server_socket.close()