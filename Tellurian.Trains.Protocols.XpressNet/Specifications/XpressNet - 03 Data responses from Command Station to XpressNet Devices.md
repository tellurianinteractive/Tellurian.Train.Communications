# XpressNet Specification - Chapter 3: Data Responses from Command Station to XpressNet Devices

**Publisher:** Lenz Elektronik GmbH

---

## 3. Data Response Commands from the Command Station to XpressNet Devices

Following are the XpressNet packets that can be transmitted from the command station to an XpressNet device. The commands include responses to requests, unrequested information, or specific information that tells the devise what is expected of it.

Within this document the following conventions are used within the tables:

- **P** = parity bit (inclusive parity of the call byte)
- **A** = device address (5 bits), XpressNet devices may have addresses 1 to 31.
- **N** = number of the data bytes in the message that follow (note: the X-Or-Byte is not included in this number)
- **GA** = XpressNet device address

Within the tables, the instruction format is indicated in decimal, hexadecimal and binary.

**Note:** When the LI100 receives a transmission it immediately transmits this information minus the call byte to the PC through its RS232 interface. This data may have been requested by a previous PC action or sent unrequested due to changes in the status of the system. Exception: The calls "acknowledgement", "TBD" and "normal inquiry" consist only of the call byte and are acted upon by the LI100 and not transmitted to the PC.

### 3.1 Normal inquiry

**Format:**

|         | Call Byte   |
|---------|-------------|
| Binary: | P10A AAAA   |
| Hex:    | P+0x40+GA   |
| Decimal:| P+64+GA     |

**Description:**

The Command Station sends a "Normal inquiry" to a specific XpressNet device enable its transmission window. Upon receipt of a "Normal inquiry" an XpressNet device can transmit a request over the XpressNet. If no transmission occurs for 80 μs, the transmission window is closed and a "Normal inquiry" is sent to another XpressNet device to enable its transmission window.

**Comments:**

This command is not sent by the LI100 to the PC.

### 3.2 Request Acknowledgement from Device

**Format:**

|         | Call Byte   |
|---------|-------------|
| Binary: | P00A AAAA   |
| Hex:    | P+0x00+GA   |
| Decimal:| P+GA        |

**Description:**

This message is sent in response to an incorrect transfer from a device to the Command Station. Upon receiving this response the XpressNet Device must respond with an "Acknowledgement". The most likely cause for this is that the received X-Or-byte was not correct.

**Flow of information when an error occurs:**

- "Normal inquiry" is transmitted by the Command Station to an XpressNet device
- The XpressNet Device transmits a request ⟹ error occurs during this transmission
- Command Station transmits "Transfer Errors" (see section 3.8)
- Command Station transmits "Request Acknowledgement from Device"
- XpressNet Device transmits "Acknowledgement Response" (see section 2.2.1)
- "Normal inquiry" is transmitted by the Command Station to an XpressNet device

**Comments:**

The call after an acknowledgement must be answered. If not answered, the command station will continue to send this request and will not accept any other requests from the device until the requests for an acknowledgement is received. If not acknowledged the command station may also decide that the device is not active, in which case it may not receive another window for an extended period.

This command is processed by the LI100 is not sent by the LI100 to the PC.

### 3.3 TBD (Future Command)

(Reserved for future use)

### 3.4 Broadcast messages

This group of messages referred to as "Broadcast" provides the possibility for the command station to transmit information to all XpressNet devices at the same time, (which includes the PC connected to an LI100). A broadcast is sent several times back to back, in order to guarantee that each participant can receive the message. Some instructions to the command station such as Stop operations request (emergency off) will result in a corresponding broadcast command. Changes in the layout environment can also result in the same command being received unrequested. Network devices, must be able to handle all broadcast commands being received at any time.

#### 3.4.1 Normal operation resumed

**Format:**

|         | Call Byte | Header byte | Data Byte 1 | X-Or-byte |
|---------|-----------|-------------|-------------|-----------|
| Binary: | P110 0000 | 0110 0001   | 0000 0001   | 0110 0000 |
| Hex:    | 0x60      | 0x61        | 0x01        | 0x60      |
| Decimal:| 96        | 97          | 1           | 96        |

**Description:**

If a network-attached device (such as a PC connected to an LI100) sends the "Resume Operation" request (see section 2.2.2), then all network-attached devices will receive a Normal operation resumed. This broadcast is a reflection of the actual condition of the system. Therefore if it is not possible to resume normal operations (for example line E sill indicating a short) then a broadcast of Emergency Stop will be sent instead!

**Comments:**

This call is received without an inquiry by the XpressNet equipment. It is an unrequested transmission that must be handled.

#### 3.4.2 Track power Off

**Format:**

|         | Call Byte | Header byte | Data Byte 1 | X-Or-byte |
|---------|-----------|-------------|-------------|-----------|
| Binary: | P110 0000 | 0110 0001   | 0000 0000   | 0110 0001 |
| Hex:    | 0x60      | 0x61        | 0x00        | 0x61      |
| Decimal:| 96        | 97          | 0           | 97        |

**Description:**

The command station sends this response when the DCC track power is switched off. No additional commands can be sent to the track until normal operations are resumed. While in an "Track power Off" state requests can still be sent to the command station. This can be used to change the state of the operations once normal operations are resumed.

**Comments:**

This call is received without an inquiry by the XpressNet equipment. It is an unrequested transmission that must be handled.

#### 3.4.3 Emergency Stop

**Format:**

|         | Call Byte | Header byte | Data Byte 1 | X-Or-byte |
|---------|-----------|-------------|-------------|-----------|
| Binary: | P110 0000 | 1000 0001   | 0000 0000   | 0110 0001 |
| Hex:    | 0x60      | 0x81        | 0x00        | 0x81      |
| Decimal:| 96        | 129         | 0           | 129       |

**Description:**

The command station is in emergency stop mode and has sent emergency stop commands to all locomotives on the track by means of a DCC Broadcast Stop command. The track power remains on, so that turnout control commands can continue to be sent, however no further locomotive commands will be sent to the layout, until the command station is instructed to restart the layout.

**Comments:**

This call is received without an inquiry by the XpressNet equipment. It is an unrequested transmission that must be handled.

#### 3.4.4 Service Mode entry

**Format:**

|         | Call Byte | Header byte | Data Byte 1 | X-Or-byte |
|---------|-----------|-------------|-------------|-----------|
| Binary: | P110 0000 | 0110 0001   | 0000 0010   | 0110 0011 |
| Hex:    | 0x60      | 0x61        | 0x02        | 0x63      |
| Decimal:| 96        | 97          | 2           | 99        |

**Description:**

This call is sent to all XpressNet devices (also the PC), when the Command Station has entered Programming Mode (also called service mode). Until the Command Station has exited service mode, no further communication will take place with any XpressNet device other than the XpressNet device that requested the system to enter service mode. I.E. if the PC requested that the Command Station enter programming mode then it can communicate further with the command station through LI100. If another XpressNet device requested that the Command Station enter service mode, then no further instruction can be sent to the LI100 until the Command Station returns to normal operation (the command station has received a "Resume operations request" from the XpressNet device that requested that service mode be entered).

**Comments:**

This call is received without an inquiry by the XpressNet equipment. It is an unrequested transmission that must be handled.

#### 3.4.5 Feedback Broadcast

**Format:**

|         | Call Byte | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | Data Byte 4 | etc. | X-Or-byte |
|---------|-----------|-------------|-------------|-------------|-------------|-------------|------|-----------|
| Binary: | P010 0000 | 0100 NNNN   | ADR_1       | DAT_1       | ADR_2       | DAT2        | etc. | X-Or-byte |
| Hex:    | 0xA0      | 0x40 + N    |             |             |             |             |      | X-Or-byte |
| Decimal:| 160       | 64 + N      |             |             |             |             |      | X-Or-byte |

**Description:**

The Command Station uses this command to tell all XpressNet devices that one or several feedback input conditions changed. This broadcast command is only sent when a change occurs. In each broadcast a minimum of one address/data pair, and a maximum of 7 address/data pairs will be transmitted for a total of up to 14 bytes per transmission (not including the header and x-Or-bytes).

ADR_x and DAT_x have same the format as described under Accessory Decoder information response described in section 3.11. XpressNet devices interested in a specific feedback response must examine all the contents of the broadcast to determine the correct condition of a specific address.

**Comments:**

This call is received without an inquiry by the XpressNet equipment. It is an unrequested transmission that must be handled.

### 3.5 Service Mode information response

After receiving a service mode request the command station is shifted into service mode. When in service mode the command station will respond to a Request for Service Mode results with one of the following responses. If the command station is not in service mode and a Request for Service Mode results request is sent by an XpressNet device, then a "Instruction not supported by command station" response is sent.

#### 3.5.1 Programming info. "short-circuit"

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 0001 0010   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x12        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 18          | X-Or-byte |

**Description:**

This response is sent if a short circuit (too high a current draw) is detected on entry to service mode. It should be assumed that that a write instruction sent to the decoder was not successful. Upon receipt of this instruction subsequent service mode requests should not be sent until the user has corrected the problem.

**Comments:**

None.

#### 3.5.2 Programming info. "Data byte not found"

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 0001 0011   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x13        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 19          | X-Or-byte |

**Description:**

A service mode read request resulted in no acknowledgement. Programming of this decoder should be broken off or tried again.

**Comments:**

none

#### 3.5.3 Programming info. "Command station busy"

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 0001 1111   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x1f        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 31          | X-Or-byte |

**Description:**

This instruction is not implemented yet.

**Comments:**

None.

#### 3.5.4 Programming info. "Command station ready"

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 0001 0001   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x11        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 17          | X-Or-byte |

**Description:**

This instruction is not implemented yet.

**Comments:**

None.

#### 3.5.5 Service Mode response for Register and Paged Mode

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0011   | 0001 0000   | EEEE EEEE   | DDDD DDDD   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x63        | 0x10        | E           | D           | X-Or-byte |
| Decimal:| P+96+GA     | 99          | 16          | E           | D           | X-Or-byte |

**Description:**

This answer is given only on request of the specific XpressNet device, which shifted the command station into service mode. The register number (or CV number if in paged mode) is returned data byte 2 (EEEE EEEE) and the data value, which returned an ack in data byte 3 (DDDD DDDD). This response is only provided in Register and PAGE modes!

**Comments:**

This response is provided to Register and PAGE mode requests. If a Direct Mode request was sent and this response was received than the command station has determined that the decoder does not support Direct Mode and has shifted into Register or Page modes to obtain the desired result. Subsequent requests to this decoder should use Register or Paged mode operations.

#### 3.5.6 Service Mode response for Direct CV mode

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0011   | 0001 0100   | CCCC CCCC   | DDDD DDDD   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x63        | 0x14        | C           | D           | X-Or-byte |
| Decimal:| P+96+GA     | 99          | 20          | C           | D           | X-Or-byte |

**Description:**

This answer is given only sent on request of the XpressNet device (e.g. the PC), which told the command station to enter service mode. The number of the CV requested is returned data byte 2 (CCCC CCCC) and the value of the date within the CV is returned in data byte 3 (DDDD DDDD). It is possible to determine the value of CVs 1 to 256 using this mode (CV256 is represented as 0).

**Comments:**

If this response is sent following a Direct Mode service mode request, it can be assumed that the decoder supports Direct Mode. This response is not provided if the decoder did not respond to the service mode request and the command station was able to process the request using Paged or Register modes. Equipment must consider this and perform subsequent service mode requests using register or PAGE modes. See section 3.5.5.

### 3.6 Command Station software-version response

Up to and including X-Bus version 2.3 the command station Software-Version response contained a single byte. Starting with XpressNet version 3.0 a second byte, which contains the command station identification, is also sent. Using this command, an XpressNet device can determine the version of X-Bus or XpressNet that the command station supports, e.g. which speed and direction commands are supported, whether multi-unit operations are possible, etc.

#### 3.6.1 Software-version(X-Bus V1 and V2)

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0010   | 0010 0001   | OOOO UUUU   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x62        | 0x21        | O + U       | X-Or-byte |
| Decimal:| P+96+GA     | 98          | 33          | O + U       | X-Or-byte |

**Description:**

This response is provided in answer to the Command station software-version request. The major version release is in the upper (OOOO) nibble and the software release is in the lower (UUUU) nibble in hexadecimal codes. Example: Data Byte 2 = 0010 0011 = 0x23: Version 2.3

**Comments:**

None.

#### 3.6.2 Software-version (XpressNet only)

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0011   | 0010 0001   | OOOO UUUU   | IIII IIII   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x63        | 0x21        | O + U       | ID          | X-Or-byte |
| Decimal:| P+96+GA     | 99          | 33          | O + U       | ID          | X-Or-byte |

**Description:**

This response is provided in answer to the Command station software-version request. The major version release is in the upper (OOOO) nibble and the software release is in the lower (UUUU) nibble in hexadecimal codes. Example: Data Byte 2 = 0011 0000 = 0x30: Version 3.0.

Additionally the command station identification is sent, which has the following meaning:

- ID = 0x00: LZ 100 - command station
- ID = 0x01: LH 200 - command station
- ID = 0x02: DPC - command station (Compact and COMM other)

**Comments:**

None.

### 3.7 Command station status indication response

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0010   | 0010 0010   | SSSS SSSS   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x62        | 0x22        | S           | X-Or-byte |
| Decimal:| P+96+GA     | 98          | 34          | S           | X-Or-byte |

**Description:**

When the command station receives a request for its status (see section 2.2.15), the command station transmits a status byte back. This byte (contained in Data Byte 2) is coded as follows:

- **Bit 0:** =1 - Command station is in emergency off
- **Bit 1:** =1 - Command station is in emergency stop
- **Bit 2:** Command station-Start mode (0 = manual mode, 1 = automatic mode)
  - Automatic Mode: All locomotives start at their last known speed each time the command station is powered up
  - Manual Mode: All locomotives have a speed of 0 and functions out on command station power up
- **Bit 3:** = 1 - The command station is in service mode
- **Bit 4:** reserved
- **Bit 5:** reserved
- **Bit 6:** = 1 - The command station is performing a power up.
- **Bit 7:** = 1 - There was a RAM check error in the command station

**Comments:**

If bit 6 and bit 2 are set, the command station is in automatic mode and has not begun to send data packets to the track. To start a command station in this mode an XpressNet device must send "start-mode manual" or "start-mode auto" to the command station. Once a command station receives one of these instructions from any XpressNet device it responds with broadcast Normal operation resumed and immediately begins sending data packets to the power stations on the control bus.

Since a command station in automatic mode will not begin operation until an XpressNet device sends a "start-mode auto" or "start-mode manual" request XpressNet devices on power up should first determine the command station status before sending operation requests. (e.g. request for loco information or operation).

Note that not all command stations support different start-modes (and therefore bits 6 and 2 of status byte).

### 3.8 Transfer Errors

**Format:**

|         | Call Byte   | Header Byte | Data Byte 1 | XOR-Byte  |
|---------|-------------|-------------|-------------|-----------|
| Binary  | P11A AAAA   | 0110 0001   | 1000 0000   | 1110 0001 |
| Hex     | P+0x60+GA   | 0x61        | 0x80        | 0xE1      |
| Decimal | P+96+GA     | 97          | 128         | X-OR-byte |

**Description:**

The Command Station sends the answer "Transfer Error" to the device that transmitted the instruction if, due to transmission or reception problems, the X-OR byte received is not correct, (e.g. the XOR computation of all bytes including the X-Or byte does not equal 0).

**Comments:**

A transfer error occurs if the XOR byte is computed incorrectly or the hardware handshake isn't taken into consideration. When using an LI100, this problem can also be caused by a buffer overflow in the PC UART hardware (caused by the driver software not processing the send and receive FIFO). As a rule, a transfer error entails additional error messages, (e.g. a timeout between the PC and LI100F). The PC interface can also received this message in response to an instruction that would not normally receive an answer other than 01/04/05.

### 3.9 Command station busy response

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 1000 0001   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x81        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 129         | X-Or-byte |

**Description:**

The command station sends the Command Station Busy response in response to a request if the request cannot be answered at the present point in time or that the command cannot be placed on the track at the present point in time.

**Comments:**

Normally a Command station busy response will not be received but it is possible if, for example, the PC this tries to switch a large number of turnouts as fast as possible on a very busy layout. The PC must look for the Command station busy response as a response to each request sent, in order to be able to send the request again should the command station be busy.

### 3.10 Instruction not supported by command station

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 1000 0010   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x82        | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 130         | X-Or-byte |

**Description:**

If an instruction received by the command station is not supported, then the command station sends this answer back. Likewise, this response is sent if the request was not allowed in the current context. For example, the PC will receive this response after sending a Direct Mode CV read request (CV mode) when the command station is not currently in service mode.

**Comments:**

None.

### 3.11 Accessory Decoder information response

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte |
|---------|-------------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0100 0010   | AAAA AAAA   | ITTN ZZZZ   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x42        | Addr        | ITNZ        | X-Or-byte |
| Decimal:| P+96+GA     | 66          | Addr        | ITNZ        | X-Or-byte |

**Description:**

The command station sends this information in response to Accessory Decoder information request. The data contained in the response can be information about the status of accessory decoders with or without feedback (such as the LS100/110), or the status of feedback modules (such as the LR100/101). A detail description of the information returned follows:

**Data Byte1: AAAA AAAA:** For a turnout accessory decoder, the address in Data Byte 1 is the turnout address divided by 4. For X-Bus V1 and X-bus V2 systems, the address has a range of 0 to 63, which equals 6 bits. XpressNet based systems have the possibility to use all 8 bits for this group address which provides a range for turnout address from 0.1023 (256 * 4 = 1024). For example, for accessory decoders, when ADR =0x00, (turnout group 0) the information is about turnouts 0, 1, 2 or 3 (accessory decoders are marked in the coding bits TT). Please note that XpressNet commands use addresses from 0 to 1023 while the operator interface commonly uses addresses from 1-1024. This difference needs to be considered in program development.

For a feedback module, the address can be in the range of 0..127 (7 Bit) and is directly the address of the module.

**Data Byte 2: I** If this bit is 1, the switching command requested has not been completed and the turnout has not reached its end position.

For feedback modules this bit will always be 0 because the inputs of these modules can become only be 0 or 1.

**Data 2: TT** This is the coding of the type of requested address:

- TT = 0 0: Address is accessory decoder without feedback
- TT = 0 1: Address is accessory decoder with feedback
- TT = 1 0: Address is a feedback module
- TT = 1 1: reserved for future use

**Data 2: N** This bit describes which nibble of a turnout or feedback module this response describes. N=0 is the lower nibble, N=1 the upper nibble. For example, for turnout group 0, if N=1 then the status flags (Z3-Z0) contain the status of turnouts 0 and 1. If N=1 the status of turnouts 3 and 4 are provided. For a feedback module the lower nibble contains status for the 4 lower feedback inputs, the upper nibble contains status of the 4 higher feedback inputs. To access all 8 inputs of a feedback module you need to request the information twice, once for the lower nibble, and once for the upper nibble. Please note that the nibble bits are only correct, if a turnout was switched during the current operating session.

**Data 3: Z3 Z2 Z1 Z0** Definitions for the status flags

In the case of a turnout accessory decoder: Z1 and Z0 represent the status of the first turnout in the nibble, Z3 and Z2 represent the status of the second turnout in the nibble.

**Possible combinations:**

| Z1 | Z0 | (the first turnout in the nibble) |
|----|----|-----------------------------------|
| 0  | 0  | turnout has not been controlled during this operating session. |
| 0  | 1  | last command sent was "0", turnout "left". This is only relative. |
| 1  | 0  | last command sent command was "1" |
| 1  | 1  | invalid combination - both end switches of a Turnout with feedback are indicating that they are active. |

The same structure is used for Z3 and Z2, the second turnout in the nibble. For a feedback module, the 4 bits Z3 toZ0 directly represent the value of the 4 inputs of the requested nibble.

**Comments:**

None.

### 3.12 Locomotive information response (X-Bus V1)

One obtains this locomotive information in response to a locomotive request to the command station as described under section 2.2.19.1. X-Bus V1 only supports 14-speed step mode and thus does not need the "ModSel" byte. If another X-Bus device takes over control of a locomotive, the information Locomotive is being operated by another device is sent unrequested to the X-Bus device, which is currently controlling the locomotive. Therefore, all X-Bus devices should have a routine for the purpose to make the user aware of the fact that control of this locomotive was just taken over by another X-Bus device.

#### 3.12.1 Locomotive is available for operation

If no other X-Bus device is currently controlling the locomotive, for which the information is being requested, then one obtains the following information with the header byte "locomotive available" as a response to a request for locomotive information.

**Format:**

|         | Call Byte   | Header byte | Data Byte 1          | Data Byte 2               | Data Byte 3               | X-Or-byte |
|---------|-------------|-------------|----------------------|---------------------------|---------------------------|-----------|
| Binary: | P11A AAAA   | 1000 0011   | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x83        | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |
| Decimal:| P+96+GA     | 131         | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |

**Description:**

**Locomotive Address:** An address within the range of values 0 - 99 dec. = 0 - 0x63. The address 0 designates a non-decoder-equipped locomotive.

**Locomotive Data Byte 1:**

- **Bit 7:** B7=1: future use: Locomotive being operated by another X-Bus device
  - B7=0: future use: Locomotive is not currently being operated by any X-Bus device

Note bit7 is not currently being used, the header byte is used to be recognized whether the locomotive is available or is being controlled by another X-Bus device!

- **Bit 6:** B6=1: last direction command was forward
  - B6=0: last direction command was reverse
- **Bit 5:** B5=1: Function 0 is switched on
  - B5=0: Function 0 is switched off
- **Bit 4:** not used, always 0.
- **Bit3 to Bit0** provides the locomotives current speed:

| Bit3 | Bit2 | Bit1 | Bit0 | Meaning        |
|------|------|------|------|----------------|
| 0    | 0    | 0    | 0    | Speed Step 0   |
| 0    | 0    | 0    | 1    | Emergency Stop |
| 0    | 0    | 1    | 0    | Speed Step 1   |
| ...  | ...  | ...  | ...  | ...            |
| 1    | 1    | 1    | 1    | Speed Step 14  |

**Locomotive Data Byte 2:**

- **Bit7 to Bit4:** not defined
- **Bit3:** Status of function 4, "0" = off, "1" = on
- **Bit2:** Status of function 3, "0" = off, "1" = on
- **Bit1:** Status of function 2, "0" = off, "1" = on
- **Bit0:** Status of function 1, "0" = off, "1" = on

**Comments:**

None.

#### 3.12.2 Locomotive is being operated by another device

If the inquired locomotive is currently being controlled by another X-Bus device, then this response is provided. The contents of the header byte differentiates this from the Locomotive is available for operation response. This response can also be received unrequested, if another X-Bus device took over control of this locomotive.

**Format:**

|         | Call Byte   | Header byte | Data Byte 1          | Data Byte 2               | Data Byte 3               | X-Or-byte |
|---------|-------------|-------------|----------------------|---------------------------|---------------------------|-----------|
| Binary: | P11A AAAA   | 1010 0011   | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xA3        | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |
| Decimal:| P+96+GA     | 163         | Locomotive address   | Locomotive Data Byte 1    | Locomotive Data Byte 2    | X-Or-byte |

**Description:**

Locomotive address, locomotive Data Byte 1 and locomotive Data Byte 2 described the format as under 3.12.1.

**Comments:**

The information "Locomotive is being operated by another device" can be received unrequested if another X-Bus device takes control of the locomotive.

### 3.13 Locomotive information response (X-Bus V2)

The locomotive information response is provided as an answer to the Locomotive information requests (X-Bus V1 and V2) as described in section 2.2.19.2. Starting with version 2.0 of the command station multiple speed step modes were supported (14, 27 and 28 speed steps). This required the locomotive information response to be extended by one byte "ModSel" (mode SELECT). This byte contains only the coding of the speed step information of the inquired locomotive.

If another X-Bus device issues a locomotive operation request for this locomotive address, the response "Locomotive is being operated by another device" is sent unrequested to the X-Bus device that was previously operating the locomotive. An appropriate operator interface response should be provided in order make the user aware of the fact that control of this locomotive was just taken over by another device.

#### 3.13.1 Locomotive is available for operation

If the locomotive is not currently being controlled by another XpressNet device the following locomotive information is provided in response to a "Locomotive information requests (X-Bus V1 and V2).

**Format:**

|         | Call Byte   | Header byte | Data Byte 1          | Data Byte 2            | Data Byte 3            | Data Byte 4 | X-Or-byte |
|---------|-------------|-------------|----------------------|------------------------|------------------------|-------------|-----------|
| Binary: | P11A AAAA   | 1000 0100   | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x84        | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |
| Decimal:| P+96+GA     | 132         | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |

**Description:**

**Locomotive Address:** A value within the range of 0 to 99 dec. = 0 to 0x63 hex. The address 0 designates a non-decoder equipped conventional locomotive.

**ModSel:** Contains the current speed step mode for the locomotive:

Depending upon contents of ModSel, locomotive Data Byte 1 has different meanings.

- Bit7 to Bit2: not used.

| Bit 1 | Bit 0 | Meaning         |
|-------|-------|-----------------|
| 0     | 0     | 14 speed step mode |
| 0     | 1     | 27 speed step mode |
| 1     | 0     | 28 speed step mode |
| 1     | 1     | reserved        |

**Locomotive Data Byte 1** Depends on the contents of ModSel

**1. ModSel = xxxxxx00 (14 Speed Step Mode)**

Locomotive Data Byte 1 and Locomotive Data Byte 2 are as described in section 3.12.1.

**2. ModSel = xxxxxx01 (27 drive position mode)**

Locomotive Data Byte 1:

- **Bit 7:** B7=1: planned: Locomotive being operated by another X-Bus device
  - B7=0: planned: Locomotive is not currently being operated by any X-Bus device

Note bit7 is not currently being used, the header byte is used to be recognized whether the locomotive is available or is being controlled by another X-Bus device!

- **Bit 6:** B6=1: last direction command was forward
  - B6=0: last direction command was reverse
- **Bit 5:** B5=1: Function 0 is switched on
  - B5=0: Function 0 is switched off
- **Bit4 to Bit0** provides the locomotives current speed: Note that Bit4 is the LSB of the locomotive's speed.

| Bit 3 | Bit 2 | Bit 1 | Bit 0 | Bit4 (!) | Meaning           |
|-------|-------|-------|-------|----------|-------------------|
| 0     | 0     | 0     | 0     | 0        | Locomotive stopped|
| 0     | 0     | 0     | 0     | 1        | not used          |
| 0     | 0     | 0     | 1     | 0        | Emergency Stop    |
| 0     | 0     | 0     | 1     | 1        | not used          |
| 0     | 0     | 1     | 0     | 0        | Speed Step 1      |
| 0     | 0     | 1     | 0     | 1        | Speed Step 2      |
| 0     | 0     | 1     | 1     | 0        | Speed Step 3      |
| ...   | ...   | ...   | ...   | ...      | ...               |
| 1     | 1     | 1     | 1     | 0        | Speed Step 27     |

**3. ModSel = xxxxxx10 (28 drive position mode)**

Locomotive Data Byte 1:

- **Bit 7:** B7=1: planned: Locomotive being operated by another X-Bus device
  - B7=0: planned: Locomotive is not currently being operated by any X-Bus device

Note bit7 is not currently being used, the header byte is used to be recognized whether the locomotive is available or is being controlled by another X-Bus device!

- **Bit 6:** B6=1: last direction command was forward
  - B6=0: last direction command was reverse
- **Bit 5:** B5=1: Function 0 is switched on
  - B5=0: Function 0 is switched off
- **Bits 4 to 0** provides the locomotives current speed: Note that Bit 4 is the LSB of the locomotive's speed.

| Bit 3 | Bit 2 | Bit 1 | Bit 0 | Bit 4 (!) | Meaning           |
|-------|-------|-------|-------|-----------|-------------------|
| 0     | 0     | 0     | 0     | 0         | Locomotive Stopped|
| 0     | 0     | 0     | 0     | 1         | not used!         |
| 0     | 0     | 0     | 1     | 0         | Emergency Stop    |
| 0     | 0     | 0     | 1     | 1         | not used!         |
| 0     | 0     | 1     | 0     | 0         | Speed Step 1      |
| 0     | 0     | 1     | 0     | 1         | Speed Step 2      |
| 0     | 0     | 1     | 1     | 0         | Speed Step 3      |
| .     | .     | .     | .     | .         |                   |
| 1     | 1     | 1     | 1     | 0         | Speed Step 27     |
| 1     | 1     | 1     | 1     | 1         | Speed Step 28     |

**Locomotive Data Byte 2:** as described in section 3.12.1

**Comments:**

None.

#### 3.13.2 Locomotive is being operated by another device

If the inquired locomotive is currently being controlled by another X-Bus device, then this response is provided. It differs from the Locomotive is available for operation response by a difference in the header byte. This response is also sent unrequested, to the X-Bus device that is currently controlling the locomotive.

**Format:**

|         | Call Byte   | Header byte | Data Byte 1          | Data Byte 2            | Data Byte 3            | Data Byte 4 | X-Or-byte |
|---------|-------------|-------------|----------------------|------------------------|------------------------|-------------|-----------|
| Binary: | P11A AAAA   | 1010 0100   | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xA4        | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |
| Decimal:| P+96+GA     | 164         | Locomotive address   | Locomotive Data Byte 1 | Locomotive Data Byte 2 | ModSel      | X-Or-byte |

**Description:**

Locomotive address, locomotive Data Byte 1 and locomotive Data Byte 2 described the format as under 3.13.1.

**Comments:**

The information "Locomotive is being operated by another device" can be received unrequested if another X-Bus device takes control of the locomotive.

### 3.14 Locomotive information response (XpressNet only)

The locomotive information response is provided in response to the general Locomotive information requests (XpressNet only) (see 2.2.19.3). There are four possible responses depending on the current operating mode of the decoder. Contrary to earlier versions the information on whether or not a locomotive is currently being operated by another network device is contained in the response. Unlike previous versions, if a locomotive is taken over by another XpressNet equipment, then this is communicated to the device that was controlling the locomotive in a separate command (see section 3.15). In addition, a new identification byte is inserted after the header byte, which serves to distinguish the different operating modes that became available in XpressNet (such as multi-unit consist control). Because the specific locomotive address that the response deals with is identical to the preceded request for locomotive information, the inquired locomotive address is no longer contained in the response.

#### 3.14.1 Locomotive information normal locomotive

This response is sent in response to a Locomotive information requests (XpressNet only) whenever the inquired locomotive is not in either a Double Header or a Multi-Unit maintained by the command station.

**Format:**

|         | Call Byte   | Header byte | Identification | Speed Byte  | Function A | Function B | X-Or-byte |
|---------|-------------|-------------|----------------|-------------|------------|------------|-----------|
| Binary: | P11A AAAA   | 1110 0100   | 0000 BFFF      | RVVV VVVV   | 000F FFFF  | FFFF FFFF  | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE4        | Identification | Speed       | FA         | FB         | X-Or-byte |
| Decimal:| P+96+GA     | 228         | Identification | Speed       | FA         | FB         | X-Or-byte |

**Description:**

**Identification Byte:** Provides the speed step mode and locomotive in use indication

- **Bit3:** B=0: Locomotive is free
  - B=1: Locomotive is being controlled by another XpressNet Device
- **Bits 0-2:** FFF Identification of the current speed step mode being used

| Bit 2 | Bit 1 | Bit 0 | Meaning         |
|-------|-------|-------|-----------------|
| 0     | 0     | 0     | 14 speed step   |
| 0     | 0     | 1     | 27 speed step   |
| 0     | 1     | 0     | 28 speed step   |
| 1     | 0     | 0     | 128 speed step  |

**Speed Byte:** Provides the current speed and direction information for the decoder

- **Bit 7:** R=1: forward direction, R=0: reverse direction.
- **Bits 0-6:** Actual speed of locomotive
  - for 14 speed step mode the bits 0-3 are used as specified in section 2.1.8.1.
  - for 27 speed step mode the bits 0-4 are used as specified in section 2.1.9.1.
  - for 28 speed step mode the bits 0-4 are used as specified in section 2.1.9.1:
  - for 128 speed step mode the bits 0-6 are used as follows:

| Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 | Meaning        |
|-------|-------|-------|-------|-------|-------|-------|----------------|
| 0     | 0     | 0     | 0     | 0     | 0     | 0     | Speed Step 0   |
| 0     | 0     | 0     | 0     | 0     | 0     | 1     | Emergency stop |
| 0     | 0     | 0     | 0     | 0     | 1     | 0     | Speed Step 1   |
| 0     | 0     | 0     | 0     | 0     | 1     | 1     | Speed Step 2   |
| ...   | ...   | ...   | ...   | ...   | ...   | ...   | ...            |
| 1     | 1     | 1     | 1     | 1     | 1     | 1     | Speed Step 126 |

**Function Byte A:** status of the functions 0 to 4. 0 0 0 F0 F4 F3 F2 F1

**Function Byte B:** status of the functions 5 to 12 F12 F11 F10 F9 F8 F7 F6 F5

a value of "1" in a function position indicates that that function is on

**Comments:**

None.

#### 3.14.2 Locomotive information for a locomotive in a multi-unit

This response is sent in response to a Locomotive information requests (XpressNet only) whenever the inquired locomotive is a member of a Multi-Unit maintained by the command station.

**Format:**

|         | Call Byte   | Header byte | Identification | Speed Byte | Function A | Function B | Data Byte 4 | X-Or-byte |
|---------|-------------|-------------|----------------|------------|------------|------------|-------------|-----------|
| Binary: | P11A AAAA   | 1110 0101   | 0001 BFFF      | RVVV VVVV  | 000F FFFF  | FFFF FFFF  | MTR         | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE5        | Identification | Speed      | FA         | FB         | MTR         | X-Or-byte |
| Decimal:| P+96+GA     | 229         | Identification | Speed      | FA         | FB         | MTR         | X-Or-byte |

**Description:**

**Identification Byte:** Bits 0-3 indicate the speed step mode of the locomotive requested and whether it is being controlled by another XpressNet device or not as described in section 3.14.1. Note: the speed step mode for a decoder can be different than the speed step mode for the entire multi-unit consist.

**Speed Byte:** As described under section 3.14.1. The speed indicates the speed of the inquired locomotive and not the Multi-unit!

**Function A, Function B Bytes:** as described under section 3.14.1.

**MTR:** This is the Multi-Unit address of the inquired locomotive. It can have a value of between 1 and 99.

**Comments:**

Since not all command stations allow for speed and direction commands to be sent to individual locomotives within a consist, speed and direction commands should be sent to the Multi-Unit address and not the locomotive address. Function instructions are to be sent to the locomotive address.

#### 3.14.3 Locomotive information for the Multi-unit address

This response is sent in response to a Locomotive information requests (XpressNet only) whenever the inquired address is a Multi-Unit address maintained by the command station.

**Format:**

|         | Call Byte   | Header byte | Identification | Speed Byte | X-Or-byte |
|---------|-------------|-------------|----------------|------------|-----------|
| Binary: | P11A AAAA   | 1110 0010   | 0010 BFFF      | RVVV VVVV  | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE2        | Identification | Speed      | X-Or-byte |
| Decimal:| P+96+GA     | 226         | Identification | Speed      | X-Or-byte |

**Description:**

**Identification Byte:** Bits 0-3 indicate the speed step mode of the multi-unit requested and whether it is being controlled by another XpressNet device or not as described in section 3.14.1.

**Speed Byte:** The speed byte is coded as described under 3.14.1. The speed reported is the speed of the Multi-unit.

**Comments:**

No function instructions can currently be sent to the multi-unit address.

#### 3.14.4 Locomotive information for a locomotive in a Double Header

This response is sent in response to a Locomotive information requests (XpressNet only) whenever the inquired locomotive is in a Double Header maintained by the command station.

**Format:**

|         | Call Byte   | Header byte | Identification | Speed Byte | Function A | Function B | AH Byte    | AL Byte    | X-Or-byte |
|---------|-------------|-------------|----------------|------------|------------|------------|------------|------------|-----------|
| Binary: | P11A AAAA   | 1110 0110   | 0110 BFFF      | RVVV VVVV  | 000F FFFF  | FFFF FFFF  | Addr High  | Addr Low   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE6        | Identification | Speed      | F0         | F1         | AH         | AL         | X-Or-byte |
| Decimal:| P+96+GA     | 230         | Identification | Speed      | F0         | F1         | AH         | AL         | X-Or-byte |

**Description:**

**Identification Byte:** Bits 0-3 indicate the speed step mode of the locomotive requested and whether it is being controlled by another XpressNet device or not as described in section 3.14.1. Note: the speed step mode for an inquired locomotive can be different than the speed step mode for the other locomotive in a Double Header.

**Speed Byte:** As described under section 3.14.1. The speed indicates the speed of the inquired locomotive and not the Double Header!

**Function A, Function B Bytes:** as described under section 3.14.1.

**AH Byte:** Highbyte of the second locomotive address of the Double Header.

**AL Byte:** Lowbyte of the second locomotive address of the Double Header

For locomotive addresses less than 100:
- Highbyte of the locomotive address is 0x00
- Lowbyte of the locomotive address is 0x00 to 0x63

For locomotive address from 100 to 9999:
- Highbyte of the locomotive address is: AH = (ADR&0xFF00)+0xC000
- Lowbyte of the locomotive address is: AL= (ADR&0x00FF)

**Comments:**

This response is only provided if the locomotive is currently in a Double Header and the XpressNet locomotive information request was sent as described in section 2.2.19.3.

### 3.15 Locomotive is being operated by another device response (XpressNet only)

This response is sent unrequsted to the XpressNet device that had control of a locomotive prior to another XpressNet device taking control using a Locomotive operations request.

**Format:**

|         | Call Byte   | Header byte | Identification | Data Byte 1   | Data Byte 2  | X-Or-byte |
|---------|-------------|-------------|----------------|---------------|--------------|-----------|
| Binary: | P11A AAAA   | 1110 0011   | 0100 0000      | Address High  | Address Low  | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE3        | 0x40           | AH            | AL           | X-Or-byte |
| Decimal:| P+96+GA     | 227         | 64             | AH            | AL           | X-Or-byte |

**Description:**

**AH:** High byte of the locomotive address.

**AL:** Low byte of the locomotive address.

For locomotive addresses in the range from 0 to 99:
- AH of the locomotive address is 0x00
- AL of the locomotive address is 0x00 to 0x63

For locomotive addresses in the range from 100 to 9999:
- AH = (ADR&0xFF00)+0xC000
- AL= (ADR&0x00FF)

**Comments:**

This information comes always unrequested, if another XpressNet device took over control of this locomotive

### 3.16 Function status response (XpressNet only)

XpressNet supports the ability for both momentary and constant on/off functions. If supported the command station is responsible for storing this information and the XpressNet device is responsible for determining the length of time that a momentary function is on. This feature is supported in Version 3 of the LZ100-Command Station. The concept of a momentary function does not change the DCC packets to the track. A momentary function is still implemented as an ON operation followed by an OFF operation. Instead this feature lets the XpressNet device extend its functionality in the operator interface. For example if a locomotive has its Horn assigned to F5 and F5 is defined as momentary, then the XpressNet device can send an ON operation when theF5 key is presses and an OFF operation when the key is released. The associated locomotive address is not sent in the response, because this response follows a specific request that includes the locomotive address.

**Format:**

|         | Call Byte   | Header byte | Identification | Data Byte 1 | Data Byte 2 | X-Or-byte |
|---------|-------------|-------------|----------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 1110 0011   | 0101 0000      | 000S SSSS   | SSSS SSSS   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE3        | 0x50           | S0          | S1          | X-Or-byte |
| Decimal:| P+96+GA     | 227         | 80             | S0          | S1          | X-Or-byte |

**Description:**

- **S0** = 0 0 0 S0 S4 S3 S2 S1 - Contains the status of the functions F0 to F4. Sx=1 indicates that the function is momentary.
- **S1** = S12 S11 S10 S9 S8 S7 S6 S5 - Contains the status of the functions F5 to F12. Sx=1 indicates that the function is momentary.

**Comments:**

None.

### 3.17 Locomotive information response for address retrieval requests (XpressNet only)

This answer is sent as a response of an address search request as described in section 2.2.25. Using this request/response sequence, the locomotive addresses within a multi-unit consist or in the command station stack can be determined.

**Format:**

|         | Call Byte   | Header byte | Identification | Data Byte 1   | Data Byte 2  | X-Or-byte |
|---------|-------------|-------------|----------------|---------------|--------------|-----------|
| Binary: | P11A AAAA   | 1110 0011   | 0011 KKKK      | Address High  | Address Low  | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE3        | 0x30 + K       | AH            | AL           | X-Or-byte |
| Decimal:| P+96+GA     | 227         | 48 + K         | AH            | AL           | X-Or-byte |

**Description:**

The identification contains the type of the locomotive address, contained in Address High / Address Low.

**Identification Byte:**

- KKKK = 0: Normal locomotive address in Data Byte 1/2
- KKKK = 1: The locomotive address in Data Byte 1/2 is in a double header
- KKKK = 2: The locomotive address in Data Byte 1/2 is a multi unit base address
- KKKK = 3: The locomotive address in Data Byte 1/2 is in a multi unit.
- KKKK = 4: If Data Byte 1/2 = 0x00 then no address was found as a result of the request.

**AH/AL:** The locomotive address AH/AL is computed as described in section 3.15.

**Comments:**

None.

### 3.18 Double Header information response (X-Bus V1)

The Double Header information response is provided in response to the Locomotive information requests (X-Bus V1) (see 2.2.19.1). X-Bus V1 only supports 14 speed step mode so there is no need for the "ModSel" byte. The Double Header occupied response is sent unrequested to the device that was operating the DH before control was taken over by a different device.

#### 3.18.1 Double Header available

If the locomotive being requested is not being operated by any other X-Bus device and is currently in a double header, then one obtains the following information in response to a Locomotive information requests (X-Bus V1).

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2          | Data Byte 3            | Data Byte 4            | Data Byte 5          | X-Or-byte |
|---------|-------------|-------------|-------------|----------------------|------------------------|------------------------|----------------------|-----------|
| Binary: | P11A AAAA   | 1100 0101   | 0000 0100   | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xC5        | 0x04        | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |
| Decimal:| P+96+GA     | 197         | 4           | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |

**Description:**

Locomotive address 1, Locomotive Data Byte 1, Locomotive Data Byte 2 and Locomotive address 2 are coded as described in section 3.12.1.

**Comments:**

XpressNet does not support commands without the "ModSel" byte. In X-Bus V2 this response is only send as a result of a request that does not use the "ModSel" option. A X-Bus V2 request for operation of a Double Header which was assembled using the X-Bus V1 format will result in a Locomotive information response (X-Bus V2) which allows the use of the new X-Bus V2 speed commands.

#### 3.18.2 Double Header occupied

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2          | Data Byte 3            | Data Byte 4            | Data Byte 5          | X-Or-byte |
|---------|-------------|-------------|-------------|----------------------|------------------------|------------------------|----------------------|-----------|
| Binary: | P11A AAAA   | 1100 0101   | 0000 0101   | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xC5        | 0x05        | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |
| Decimal:| P+96+GA     | 197         | 5           | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | X-Or-byte |

**Description:**

Locomotive address 1, locomotive Data Byte 1, locomotive Data Byte 2 and locomotive address 2 are coded as described in section 3.12.1.

**Comments:**

The information can be received by X-Bus equipment unrequested.

### 3.19 Double Header information response (X-Bus V2)

The Double Header information response (X-Bus V2) is sent as a result of a locomotive request as described in section 2.2.19.2. Because of the possibility that the decoder is operating in 14, 27 or 28 speed steps, the "ModSel" byte is included in this response. The information "Double Header occupied" is sent unrequested to the equipment, which was controlling the locomotive before another device took over control.

#### 3.19.1 Double Header available

If the locomotive being requested is not being operated by any other X-Bus device and is currently in a double header, then one obtains the following information in response to a Locomotive information requests (X-Bus V1 and V2).

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2          | Data Byte 3            | Data Byte 4            | Data Byte 5          | Data Byte 6 | X-Or-byte |
|---------|-------------|-------------|-------------|----------------------|------------------------|------------------------|----------------------|-------------|-----------|
| Binary: | P11A AAAA   | 1100 0110   | 0000 0100   | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | ModSel      | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xC6        | 0x04        | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | ModSel      | X-Or-byte |
| Decimal:| P+96+GA     | 198         | 4           | Locomotive address 1 | Locomotive Data Byte 1 | Locomotive Data Byte 2 | Locomotive address 2 | ModSel      | X-Or-byte |

**Description:**

Locomotive address 1, locomotive Data Byte 1, locomotive Data Byte 2, locomotive address 2 and ModSel are coded as described in section 3.13.1.

**Comments:**

Double header information in this format is only sent to devices which request information in the old X-Bus V2 format that includes the "ModSel" byte. A XpressNet request for operation of a Double Header which was assembled using the X-Bus V2 format will result in a Locomotive information response (XpressNet only) which allows the use of the new XpressNet speed commands.

In XpressNet 4.0 the older speed and direction commands will longer be supported.

#### 3.19.2 Double Header occupied

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | Data Byte 2          | Data Byte 3  | Data Byte 4  | Data Byte 5          | Data Byte 6 | X-Or-byte |
|---------|-------------|-------------|-------------|----------------------|--------------|--------------|----------------------|-------------|-----------|
| Binary: | P11A AAAA   | 1100 0110   | 0000 0101   | Locomotive address 1 | Loco data 1  | Loco data 2  | Locomotive address 2 | ModSel      | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xC6        | 0x05        | Address 1            | Loco data 1  | Loco data 2  | Address 2            | ModSel      | X-Or-byte |
| Decimal:| P+96+GA     | 198         | 5           | Address 1            | Loco data 1  | Loco data 2  | Address 2            | ModSel      | X-Or-byte |

**Description:**

Locomotive address 1, locomotive Data Byte 1, locomotive Data Byte 2, locomotive address 2 and ModSel are coded as described in section 3.13.1.

**Comments:**

The information "Double Header occupied" can be received by XpressNet equipment unrequested.

### 3.20 Double Header error response (X-Bus V1 and V2)

A Double Header can only be installed or dissolved, if certain conditions are followed. If these conditions are not met one of the following error messages may be received.

**Format:**

|         | Call Byte   | Header byte | Data Byte 1 | X-Or-byte |
|---------|-------------|-------------|-------------|-----------|
| Binary: | P11A AAAA   | 0110 0001   | 1000 0FFF   | X-Or-byte |
| Hex:    | P+0x60+GA   | 0x61        | 0x80 + F    | X-Or-byte |
| Decimal:| P+96+GA     | 97          | 128 + F     | X-Or-byte |

**Description:**

The three error bits (FFF) are coded as follows:

- FFF = 011: One of the locomotives has not been operated by the XpressNet device assembling the Double Header or locomotive 0 was selected.
- FFF = 100: One of the locomotives of the Double Header is being used by another XpressNet device.
- FFF = 101: One of the locomotives is already in another Double Header.
- FFF = 110: The speed of one of the locomotives is not zero.

**Comments:**

If the operations "Establish Double Header" or "Dissolve Double Header" are successful there is no response from the command station. The LI100 announces success to the PC by the release for the next instruction. However if the operation failed, the command station transmits the error message using this response.

### 3.21 XpressNet MU+DH error message response

Starting from with XpressNet 3.0 errors are summarized in a specific error message response. The error messages transmitted is a result of a specific failure from that preceding request to the command station.

**Format:**

|         | Call Byte   | Header byte | Identification | X-Or-byte |
|---------|-------------|-------------|----------------|-----------|
| Binary: | P11A AAAA   | 1110 0001   | 1000 FFFF      | X-Or-byte |
| Hex:    | P+0x60+GA   | 0xE1        | 0x80 + F       | X-Or-byte |
| Decimal:| P+96+GA     | 225         | 128 + F        | X-Or-byte |

**Description:**

The 4 error bits (FFFF) are coded as follows:

- FFFF = 0001: One of the locomotives has not been operated by the XpressNet device assembling the Double Header/Multi Unit or locomotive 0 was selected.
- FFFF = 0010: One of the locomotives of the Double Header/Multi-Unit is being operated by another XpressNet device.
- FFFF = 0011: One of the locomotives already is in another Multi-Unit or Double Header.
- FFFF = 0100: The speed of one of the locomotives of the Double Header/Multi-Unit is not zero.
- FFFF = 0101: The locomotive is not in a multi-unit.
- FFFF = 0110: The locomotive address is not a multi-unit base address.
- FFFF = 0111: It is not possible to delete the locomotive.
- FFFF = 1000: The command station stack is full

**Comments:**

None
