# XpressNet Specification - Chapter 2: Communications between the DCC Command Station and XpressNet Devices

**Publisher:** Lenz Elektronik GmbH

---

## 2. Communications between the DCC Command Station and XpressNet Devices

A device is only allowed to transmit on the XpressNet network when it receives a transmission window. In most cases this window is controlled by the command station who acts as the manager of the XpressNet communications. Each request and response is sent in a packet. An XpressNet packet consists of a series of 8 bit bytes. To open up a transmission window, the command station transmits a Call Byte to the device. The call byte includes a parity bit, this parity bit only refers to the parity of the call byte. Following the Call Byte the command station may also transmit an instruction identifier (header byte) which describes the contents of the transmission that follows. The header byte consists of two parts with each part consisting of a 4-bit Nibble. The upper Nibble contains the function to be performed; the lower Nibble contains the number of data bytes in the packet. The number of data bytes in the packet does not include the call byte, the header byte or the error byte.

The header byte is followed by up to 15 data bytes and concluded by an X-OR error detection byte. This X-Or-byte serves as the error control for the transmission. The X-Or-byte is calculated using all bytes in the message with the exception of the Call byte and the X-Or-byte. Likewise each XpressNet transmission initiated by an XpressNet device also contains an X-Or-byte calculated in the same manner. Exception: The calls "Normal inquiry", "Request Acknowledgement from Device", and "TBD (Future Command)" consist only of the call byte and do not include either a header byte or an X-Or-byte.

An XpressNet device receives information (e.g. status of a locomotive) from the command station, by sending a specific inquiry to the command station, or by receiving an unrequested message from the command station as a result of the action of another XpressNet device (for example another device taking control of a locomotive)

XpressNet devices can only sent a single request over the XPressNet after receiving a call byte from the command station. The response is normaly transmitted back in the same window. Upon completion of the communication between the XpressNet device and the command station, the command station sends a call byte to the next XpressNet device in its queue.

It is also possible for a device to transmit a command to a devise other then the command station. Depending on the header byte of the instruction, the command station may process the request, broadcast the information to al XpressNet devices or pass on the information along to another XPressNet device for processing.


