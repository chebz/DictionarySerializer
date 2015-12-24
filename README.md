Did life get you down? Do you feel like you are missing something important to be happy? Well look no further! Custom serializers that store type information are here! Why would you ever need them? I don't know! But it's open-source, so you can add your own serializers or add your own types and become a better human being!

1. Dictionary Serializer:
	Serializes your classes to Dictionary(string, object) where string is a field name and object is a primitive data type. I use it with Photon Server protocol.
	
2. Document Serializer:
	Converts your classes to AWS DynamoDB Document type and back. Now you can store your classes in aws database (without using their DataModel pipeline that totally disrepects abstract classees). Woo!
