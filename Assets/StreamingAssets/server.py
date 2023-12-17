import sys
import os
import logging
import subprocess

def install_required_libraries():
    # Liste des bibliothèques à installer (remplacez par vos bibliothèques requises)
    required_libraries = ['keras', 'numpy', 'tensorflow', 'torch', 'zmq']

    for library in required_libraries:
        try:
            # Exécute la commande pip pour installer la bibliothèque
            subprocess.check_call([sys.executable, "-m", "pip", "install", library])
        except subprocess.CalledProcessError:
            print(f"Erreur lors de l'installation de la bibliothèque {library}")
            sys.exit(1)

# Appel de la fonction pour installer les bibliothèques requises
install_required_libraries()

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

import numpy as np
import torch
import torch.nn as nn
import torch.nn.functional as F
import zmq

class Dictionary(object):
  # To start in 0 or 1?

    def __init__(self):
        self.word2idx = {}  # if word2idx["hello"] == 42 ...
        self.idx2word = []  # ... then idx2word[42] == "hello"

    def add_word(self, word):
        """
        This function should check if the word is in word2idx; if it
        is not, it should add it with the first available index
        """
        if word not in self.word2idx:
          self.word2idx[word] = len(self.idx2word) # It starts in 0
          self.idx2word.append(word)
          return self.word2idx[word]
        else:
          return None

    def __len__(self):
        return len(self.idx2word)

class Corpus(object):
  # Dictionnary to token texts train, valid, test
    def __init__(self, path):

    # What is sos token ?

        # We create an object Dictionary associated to Corpus
        self.dictionary = Dictionary()
        self.eos_token = '<eos>'  # end of senetence (line) token
        # self.sos_token = '<sos>'
        # We go through all files, adding all words to the dictionary
        self.train = self.tokenize(os.path.join(path, 'train.txt'))
        self.valid = self.tokenize(os.path.join(path, 'valid.txt'))
        self.test = self.tokenize(os.path.join(path, 'test.txt'))


    def tokenize(self, path):
        """
            Tokenizes a text file, knowing the dictionary, in order to
            tranform it into a list of indices.
            The str.split() function might be useful.
        """
        # Add words to the dictionary
        # Do not forget to add the sos and eos tokens too

        assert os.path.exists(path)
        with open(path, 'r') as f:
          lines = f.readlines()

        for line in lines:
          for word in line.split():
            self.dictionary.add_word(word)

        self.dictionary.add_word(self.eos_token)
        # self.dictionary.add_word(self.sos_token)

        # Once done, effectively tokenize by adding the tokens to a vector.
        # We want the `ids` vector to be an int64 torch tensor containing all
        # tokens in the order of the file.
        # Lines should all end with the sos token.

        # TODO
        ids = []

        for line in lines:
          for word in line.split():
            ids.append(self.dictionary.word2idx[word])
          ids.append(self.dictionary.word2idx[self.eos_token])

        ids = np.int64(ids)
        return ids

class LSTMModel(nn.Module):
    def __init__(self, ntoken, ninp, nhid, nlayers, dropout=0.2, initrange=0.1):
        """
            ntoken: length of the dictionary,
            ninp: dimension of the input,
            nhid: dimension of the hidden layers,
            nlayers: number of layers,
            dropout: regularization parameter
            initrange: range for weight initialization
        """
        super().__init__()
        self.ntoken = ntoken
        self.nhid = nhid
        self.nlayers = nlayers
        self.initrange = initrange
        # Create a dropout object to use on layers for regularization
        self.drop = nn.Dropout(dropout)
        # Create an encoder - which is an embedding layer
        self.encoder = nn.Embedding(ntoken, ninp)
        # Create the LSTM layers - find out how to stack them !
        self.rnn = nn.LSTM(ninp, nhid, nlayers, dropout=dropout)
        # Create what we call the decoder: a linear transformation to map the hidden state into scores for all words in the vocabulary
        # (Note that the softmax application function will be applied out of the model)
        self.decoder = nn.Linear(nhid, ntoken)

        # Initialize non-recurrent weights
        self.init_weights()

    def init_weights(self):
        # Initialize the encoder and decoder weights with the uniform distribution,
        # between -self.initrange and +self.initrange, and the decoder bias with zeros
        # https://pytorch.org/docs/stable/nn.init.html?highlight=init
        # - the methods uniform_() and zeros_() might help
        # - self.encoder has a .weight attribute
        # - self.decoder has .weight and .bias attributes

        nn.init.uniform_(self.encoder.weight, -self.initrange, self.initrange)
        nn.init.zeros_(self.decoder.bias)
        nn.init.uniform_(self.decoder.weight, -self.initrange, self.initrange)

    def init_hidden(self, bsz):
        weight = next(self.parameters())
        return (weight.new_zeros(self.nlayers, bsz, self.nhid),
                weight.new_zeros(self.nlayers, bsz, self.nhid))

    def forward(self, input, hidden1):

        # Process the input with the encoder, then dropout
        emb = self.drop(self.encoder(input))

        # Apply the LSTMs
        output, hidden2 = self.rnn(emb, hidden1)

        # Decode into scores
        output = self.drop(output)
        decoded = self.decoder(output)

        return decoded, hidden2

def generate(source, n_words, temperature=1, topk=10):
    """
        n_words: number of words to generate
        fout: optional output file
    """
    path = os.path.dirname(os.path.abspath(__file__))
    corpus = Corpus(os.path.join(path, "save_files"))
    vocab_to_int = corpus.dictionary.word2idx
    int_to_vocab = corpus.dictionary.idx2word
    model.eval()
    softmax = nn.Softmax(dim=-1)
    source = source.split()
    hidden = model.init_hidden(1)
    for v in hidden:
        v = v.to("cpu")
    for w in source:
        ix = torch.tensor([[vocab_to_int[w]]]).to("cpu")
        output, hidden = model(ix, hidden)
    output = output / temperature # Here there is only last output of the for
    # To change output and idx_max (ix)
    # To input whole list instead of last item of output

    if topk > 0:
        probas = softmax(torch.topk(softmax(output[0]), topk).values[0]).cpu().detach().numpy()
        indices = torch.topk(softmax(output[0]), topk).indices[0].cpu()
        idx_max = np.random.choice(indices, 1, p=probas)[0]
    else:
        idx_max = torch.argmax(softmax(output[0]))
    words = []
    words.append(int_to_vocab[idx_max])
    for i in range(1, n_words):
        ix = torch.tensor([[idx_max]]).to("cpu")
        output, hidden = model(ix, hidden)
        output = output / temperature
        if topk > 0:
            probas = softmax(torch.topk(softmax(output[0]), topk).values[0]).cpu().detach().numpy()
            indices = torch.topk(softmax(output[0]), topk).indices[0].cpu()
            idx_max = np.random.choice(indices, 1, p=probas)[0]
        else:
            idx_max = torch.argmax(softmax(output[0]))
        words.append(int_to_vocab[idx_max])
    text = " ".join(words)
    text = text.replace("<eos>", "\n")
    # pp.pprint(text)
    return text

print("Version de Python en cours d'execution :")
print(sys.version)
try:
    model_path = sys.argv[1]
    with open(model_path, 'rb') as f:
            model = torch.load(f, map_location=torch.device("cpu"))
            # after load the rnn params are not a continuous chunk of memory
            # this makes them a continuous chunk, and will speed up forward pass
            model.rnn.flatten_parameters()
            print("Successfully loaded model from {}".format(model_path))

    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5555")

    source = """i am the best man in the world"""
    while True:
        bytes_received = socket.recv(3136 * 10)
        received_string = bytes_received.decode('utf-8')
        new_string = ""+received_string+""
        if new_string:
            pred = new_string + " " + generate(new_string, n_words=10, temperature=1, topk=10)
            bytes_to_send = pred.encode('utf-8')
            socket.send(bytes_to_send)
            sys.stdout.flush()  # Flush the print output to ensure it's sent immediately
except Exception as e:
    # En cas d'erreur, enregistrez-la dans les logs
    logging.error(str(e), exc_info=True)