FROM python:3.12

COPY ./azurite .

RUN ls -lah

RUN pip install azure-storage-blob

CMD python3 -u ./azurite-setup.py
