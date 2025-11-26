# XpressNet Specification - Chapter 4: Data messages from XpressNet Device to Command Station

**Publisher:** Lenz Elektronik GmbH

---

## 4. Data sent from an XpressNet device to the command station

Upon receipt of a "Normal inquiry" (see section 3.1), an XpressNet device can begin transmission and make a request e.g. ask for data, control locomotives or execute programming sequences. Depending upon the specific request, the command station may provide the appropriate response to the device.

Only one request can be processed after each "Normal inquiry", i.e. a XpressNet device cannot transmit a locomotive control instruction to the head office and inquire immediately thereafter the status of a locomotive. Before the second request can be transmitter the XpressNet device must receive another "Normal inquiry". Between a "Normal inquiry" to the same XPressNet device, the command station will likely send a "Normal inquiry" to other XpressNet devices on the Network. Thus is to guarantee that a single device cannot consume the entire bandwidth and that all devices connected to the network are guaranteed a transmission window.

After the LI100 provides the CTS signal (and only then), can the PC initiate communication to control devices (such as locomotives) or request information. Depending on the actual PC requests the LI100 either forwards these requests on to the command station or provides the response itself. If the request results in a needed response from the command station, the command station sends the appropriate response to the LI and the LI forwards this information to the PC. It is important that by the PC strictly utilize the hardware handshake of the serial interface because this is the only means that the LI100 uses to indicate when it is ready to receive communication.

When there is any trouble in receiving something during the time slot the addressed device have to respond the LZ sends a "quit" command to that device immediately. The device now is informed and can make some further decisions.

**Note:** Not all command stations support all instructions. This must be considered by all XpressNet device including a PC program, in order to avoid continuous loops or improper operation (see also section 3.10 Instruction not supported by command station).

If the command station received the request but can not process the command station transmits a "Command station busy response" described in section 3.8 or an "Instruction not supported by command station" response described in section 3.10 back to the XpressNet device.

### 4.1 Acknowledgement Response

**Format:**

|         | Header byte | X-Or-byte  |
|---------|-------------|------------|
| Binary: | 0010 0000   | 0010 0000  |
| Hex:    | 0x20        | 0x20       |
| Decimal:| 32          | 32         |

**Description:**

If a device receives a "Request Acknowledgement from Device", then it must answer with this response. The command station will continue to send a "Request Acknowledgement from Device" and will allow no further communications until it receives this answer.

**Comments:**

This transmission is performed by the actual XpressNet device. In the case of a PC connected to an LI100, the LI100 transmits the acknowledgement and not the PC.

### 4.2 Resume operations request

**Format:**

|         | Header byte | Data Byte 1 | X-Or-byte  |
|---------|-------------|-------------|------------|
| Binary: | 0010 0001   | 1000 0001   | 1010 0000  |
| Hex:    | 0x21        | 0x81        | 0xA0       |
| Decimal:| 33          | 129         | 160        |

**Description:**

The instruction tells the command station to turn on power to the track (if it was switched off) and begin transmitting DCC packets to the track. This causes the termination of an emergency stop, an emergency off or the completion of the programming on the programming track (service mode) operations.

**Comments:**

None.

**Response:**

Upon completion of this command the command station sends Normal operation resumed as a broadcast message to all XpressNet devices. See section 3.4.1.

### 4.3 Stop operations request (emergency off)

**Format:**

|         | Header byte | Data Byte 1 | X-Or-byte  |
|---------|-------------|-------------|------------|
| Binary: | 0010 0001   | 1000 0000   | 1010 0001  |
| Hex:    | 0x21        | 0x80        | 0xA1       |
| Decimal:| 33          | 128         | 161        |

**Description:**

This instruction tells the command station to immediately stop sending DCC packets to the track and to switch off the DCC track power.

**Comments:**

None.

**Response:**

After turning off the track power, the command station sends Track power Off as a broadcast message several times to all XpressNet devices. See section 3.4.2

### 4.4 Stop all locomotives request (emergency stop)

**Format:**

|         | Header byte | X-Or-byte  |
|---------|-------------|------------|
| Binary: | 1000 0000   | 1000 0000  |
| Hex:    | 0x80        | 0x80       |
| Decimal:| 128         | 128        |

**Description:**

The instruction instructs the command station to immediately emergency stop all locomotives on the layout. The DCC track power remains switched on, so that turnouts can continue to be controlled.

**Comments:**

None.

**Response:**

The command station sends a broadcast of "everything stopped" several times to all network devices and also to the specific XpressNet device that initiated this instruction. See section 3.4.3.

### 4.5 Emergency stop a specific locomotive operation

#### 4.5.1 Emergency stop a locomotive (X-Bus V1 and V2)

**Format:**

|         | Header byte | Data Byte 1          | X-Or-byte  |
|---------|-------------|----------------------|------------|
| Binary: | 1001 0001   | Locomotive address   | X-Or-byte  |
| Hex:    | 0x91        | Locomotive address   | X-Or-byte  |
| Decimal:| 145         | Locomotive address   | X-Or-byte  |

**Description:**

The instruction tells the command station to immediately stop only the desired locomotive on the track without its preprogrammed deceleration rate (emergency stop). The DCC track power remains switched on, so that turnouts and all other locomotives can continue to be controlled normally.

**Comments:**

Locomotive addresses in the range of 0 to 99 are allowed.

**Response:**

None

#### 4.5.2 Emergency stop a locomotive (XpressNet)

**Format:**

|         | Header byte | Data Byte 1  | Data Byte 2 | X-Or-byte  |
|---------|-------------|--------------|-------------|------------|
| Binary: | 1001 0010   | Address High | Address Low | X-Or-byte  |
| Hex:    | 0x92        | AH           | AL          | X-Or-byte  |
| Decimal:| 146         | AH           | AL          | X-Or-byte  |

**Description:**

The instruction tells the command station to immediately stop only the desired locomotive on the track without its preprogrammed deceleration rate (emergency stop). The DCC track power remains switched on, so that turnouts and all other locomotives can continue to be controlled normally.

**Comments:**

Locomotive addresses in the range of 0 to 99 are allowed.

The locomotive address AH/AL is as specified in section 3.14.

**Response:**

None

### 4.6 Emergency stop selected locomotives (X-Bus V1 and V2)

**Format:**

|         | Header byte | Data Byte 1            | .. | Data Byte N            | X-Or-byte  |
|---------|-------------|------------------------|----|------------------------|------------|
| Binary: | 1001 NNNN   | Locomotive address 1   | .. | Locomotive address N   | X-Or-byte  |
| Hex:    | 0x90 + N    | Locomotive address 1   | .. | Locomotive address N   | X-Or-byte  |
| Decimal:| 144 + N     | Locomotive address 1   | .. | Locomotive address N   | X-Or-byte  |

**Description:**

The instruction tells the command station to immediately stop only the specified locomotives on the track without their preprogrammed deceleration rate (emergency stop). The DCC track power remains switched on, so that turnouts and all other locomotives can continue to be controlled normally.

**Comments:**

Locomotive addresses in the range of 0 to 99 are allowed.

This request is no longer supported in XpressNet and should be replaced by a sequence of "Emergency stop a locomotive" requests.

**Response:**

None

### 4.7 Register Mode read request (Register Mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|-------------|-------------|------------|
| Binary: | 0010 0010   | 0001 0001   | 0000 RRRR   | X-Or-byte  |
| Hex:    | 0x22        | 0x11        | R           | X-Or-byte  |
| Decimal:| 34          | 17          | R           | X-Or-byte  |

**Description:**

The request tells the command station to switch into service mode and to read the decoder that is on the programming track using Register Mode. The command station attempts to read the register, which is indicated as 0000 RRRR. Values of 1..8 are allowed.

**Comments:**

The read instruction does not require an answer by the command station! A result must be specifically requested with the "Request for Service Mode results" request. Only after receiving the response to programming results request can it be determined whether the read instruction was successful or not.

When the command station receives a register read instruction the command station sends the Broadcast "service mode entry" to all network participants which prevents them from sending further instructions. Only the XpressNet device that requested the Command Station enter service mode can continue to send further service mode requests until that device instructs the command station to exit service mode.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.8 Direct Mode CV read request (CV mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|-------------|-------------|------------|
| Binary: | 0010 0010   | 0001 0101   | CCCC CCCC   | X-Or-byte  |
| Hex:    | 0x22        | 0x15        | CV          | X-Or-byte  |
| Decimal:| 34          | 21          | CV          | X-Or-byte  |

**Description:**

The request tells the command station to switch into service mode and to read the decoder that is on the programming track, using Direct CV Mode. The CV read is specified by the value of CCCC CCCC.

The range is from 1 to 256, CV256 is sent as 00

**Comments:**

The read instruction does not require an answer by the command station! A result must be specifically requested with the request "programming result". Only after receiving the response to programming results request can it be determined whether the read instruction was successful or not or if the decoder supports Direct CV mode. If the decoder could not be read using Direct CV mode, the command station may try register mode. The results of these read actions are provided in response to a "Request for Service Mode results" request. The XpressNet equipment must examine the results to determine which mode was used to read the CV.

When the command station receives a direct CV service mode read instruction the command station sends the Broadcast "service mode entry" to all network participants which prevents them from sending further instructions. Only the XpressNet device that requested the Command Station enter service mode can continue to send further service mode requests until that device instructs the command station to exit service mode.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.9 Paged Mode read request (Paged Mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|-------------|-------------|------------|
| Binary: | 0010 0010   | 0001 0100   | CCCC CCCC   | X-Or-byte  |
| Hex:    | 0x22        | 0x14        | CV          | X-Or-byte  |
| Decimal:| 34          | 20          | CV          | X-Or-byte  |

**Description:**

This request tells the command station to switch into service mode and to read the decoder that is on the programming track using PAGED mode. It is the responsibility of the command station to make the appropriate conversion of the CV into the page and offset, and to set the page register appropriately before attempting the value of the appropriate register.

The CV read is specified by the value of CCCC CCCC. The range is from 1 to 256, whereby CV256 is to be sent as 00

**Comments:**

The read instruction does not require an answer by the command station! A result must be specifically requested with the request "programming result". Only after receiving the response to programming results request can it be determined whether the read instruction was successful and if the requested mode way used. The results of these read actions are provided in response to a "Request for Service Mode results" request. The XpressNet equipment must examine the results to determine which mode was used to read the CV.

When the command station receives a paged mode read instruction the command station sends the Broadcast "service mode entry" to all network participants which prevents them from sending further instructions. Only the XpressNet device that requested the Command Station enter service mode can continue to send further service mode requests until that device instructs the command station to exit service mode.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.10 Request for Service Mode results

**Format:**

|         | Header byte | Data Byte 1 | X-Or-byte  |
|---------|-------------|-------------|------------|
| Binary: | 0010 0001   | 0001 0000   | 0011 0001  |
| Hex:    | 0x21        | 0x10        | 0x31       |
| Decimal:| 33          | 16          | 49         |

**Description:**

The instruction requests the command station to transmit back the result of a proceeding read action to the XpressNet equipment.

**Comments:**

None.

**Response:**

The response is described in section 3.5.

### 4.11 Register Mode write request (Register Mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|-------------|-------------|-------------|------------|
| Binary: | 0010 0011   | 0001 0010   | 0000 RRRR   | Data        | X-Or-byte  |
| Hex:    | 0x23        | 0x12        | R           | Data        | X-Or-byte  |
| Decimal:| 35          | 18          | R           | Data        | X-Or-byte  |

**Description:**

The service mode instruction tells the command station switch into service mode and to write the specified value (Data Byte 3) into the specified register (Data Byte 2) using Register Mode.

The range of register values is 1 to 8.

**Comments:**

Before a write instruction is used, the command station should be shifted by a read instruction into the programming mode. There is no control on the part of the XpressNet equipment over it, whether the decoder also understood the programming sequence, except by repeated selection.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.12 Direct Mode CV write request (CV mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|-------------|-------------|-------------|------------|
| Binary: | 0010 0011   | 0001 0110   | CCCC CCCC   | Data        | X-Or-byte  |
| Hex:    | 0x23        | 0x16        | CV          | Data        | X-Or-byte  |
| Decimal:| 35          | 22          | CV          | Data        | X-Or-byte  |

**Description:**

This service mode request tells the command station to switch into service mode and to write the specified value (Data Byte 3) into the specified CV (Data Byte 2) using Direct CV Mode.

The range for CVs is 1-256. CV256 is requested using a value of 0x00 in Data Byte 2.

**Comments:**

Before a direct mode CV write request is issued, a direct mode CV read instruction should be issued and the service mode results examined in order to determine that the decoder supports Direct Mode. It is the responsibility of the XpressNet device to determine that the decoder supports a specific mode before using that mode.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.13 Paged Mode write request (Paged mode)

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|-------------|-------------|-------------|------------|
| Binary: | 0010 0011   | 0001 0111   | CCCC CCCC   | Data        | X-Or-byte  |
| Hex:    | 0x23        | 0x17        | CV          | Data        | X-Or-byte  |
| Decimal:| 35          | 23          | CV          | Data        | X-Or-byte  |

**Description:**

The service mode instruction tells the command station switch into service mode and to write the specified value (Data Byte 3) into the specified CV (Data Byte 2) using Paged Mode. It is the responsibility of the command station to make the appropriate conversion of the CV into the Page and offset, and to set the page register appropriately before setting the value of the desired register.

The CV read is specified by the value of CCCC CCCC. The range for CVs is 1-256. CV256 is requested using a value of 0x00 in Data Byte 2.

**Comments:**

It is not possible to determine if a decoder supports paged mode. Therefore care must be taken with this mode, as an unexpected result will occur if the decoder does not support paged mode.

**Response:**

A "service mode entry" broadcast is sent to all XpressNet devices upon the command stations entry to service mode. See section 3.4.4

### 4.14 Command station software-version request

**Format:**

|         | Header byte | Data Byte 1 | X-Or-byte  |
|---------|-------------|-------------|------------|
| Binary: | 0010 0001   | 0010 0001   | 0000 0000  |
| Hex:    | 0x21        | 0x21        | 0x00       |
| Decimal:| 33          | 33          | 0          |

**Description:**

This request instructs the command station to respond with its software version number.

**Comments:**

None.

**Response:**

The response is described in section 3.6.

### 4.15 Command station status request

**Format:**

|         | Header byte | Data Byte 1 | X-Or-byte  |
|---------|-------------|-------------|------------|
| Binary: | 0010 0001   | 0010 0100   | 0000 0101  |
| Hex:    | 0x21        | 0x24        | 0x05       |
| Decimal:| 33          | 36          | 5          |

**Description:**

This request instructs the command station to respond with its current status.

**Comments:**

None.

**Response:**

The response is described in section 3.7.

### 4.16 Set command station power-up mode

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|-------------|-------------|------------|
| Binary: | 0010 0010   | 0010 0010   | 0000 0M00   | X-Or-byte  |
| Hex:    | 0x22        | 0x22        | M           | X-Or-byte  |
| Decimal:| 34          | 34          | M           | X-Or-byte  |

**Description:**

Sets the starting mode of the command station when it is powered up. M=0: Manual Start Mode - no speed and direction commands are sent to locomotives on power up, M=1: Automatic Start Mode - on power up the command station sends DCC packets to all known locomotives with the last known speed, direction and function status.

**Comments:**

Not all command stations support this request.

**Response:**

None

### 4.17 Accessory Decoder information request

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|-------------|-------------|------------|
| Binary: | 0100 0010   | AAAA AAAA   | 1000 000N   | X-Or-byte  |
| Hex:    | 0x42        | Address     | 0x80 + N    | X-Or-byte  |
| Decimal:| 66          | Address     | 128 + N     | X-Or-byte  |

**Description:**

This request instructs the command station to respond with the accessory decoder status.

**Address:** For a switching decoder, this is the address of the desired output divided by 4 (group address). The range of the group address is 0..63 = 6bit for central unit versions < 3.0.

For version 3.0 and above, all 8 bits are allowed for the group address. So 1024 turnouts can be switched and turnouts 0..512 can have feedback, turnouts 513..1023 can not have feedback.

For a feedback module, the address can be 0..127 and is the module address.

**N:** Marks the desired nibble. N=0 is the lower nibble, N=1 the upper nibble.

For switching decoders, there are 4 turnouts in one group and the lower nibble marks turnouts 0 and 1 in the group and the upper nibble turnouts 2 and 3.

For a feedback module, in the lower nibble the 4 lower inputs of the module are included and in the upper nibble, the upper 4 inputs.

**Comments:**

**Example 1:** Status of turnout 21 is desired. Address 21 div by 4 = 5. So turnout 21 is in group 5. Turnouts 20,21,22 and 23 are in group 5, so the nibble bit is 0 (lower nibble).

**Example 2:** Status of turnout 620 is desired. Address 620 div by 4 = 155. So turnout 620 is in group 155. Turnouts 620, 621, 622 and 623 are in group 155, so the nibble bit is 1 (higher nibble).

**Response:**

The response is described in section 3.11.

### 4.18 Accessory Decoder operation request

**Format:**

|         | Header byte | Data Byte 1 | Data Byte 2    | X-Or-byte  |
|---------|-------------|-------------|----------------|------------|
| Binary: | 0101 0010   | AAAA AAAA   | 1000 DBBD      | X-Or-byte  |
| Hex:    | 0x52        | Address     | 0x80 + DBBD    | X-Or-byte  |
| Decimal:| 82          | Address     | 128 + DBBD     | X-Or-byte  |

**Description:**

Switching commands can only be sent to switching decoders. The address is turnout / 4 (group). The offset in the group and which of the 2 outputs has to be activated or deactivated has to be defined. This is done with the bits D1 B1 B0 and D2 in data 2.

**B1 and B0:** These are the two LSB´s which are the rest of the division by 4.

**D1:** D1 = 0 means activate output.
       D1 = 1 means deactivate output.

**D2:** D2 = 0 means use output 1 of the selected turnout.
       D2 = 1 means use output 2 of the selected turnout.

**Comments:**

Prior to XpressNet only ab decoder address range from 0 to 63 was defined as a valid address. XpressNet allows the full range from 0-255 to be used. Not all command stations support this full range. See also section 4.17.

**Response:**

None

### 4.19 Locomotive information request

#### 4.19.1 Locomotive information requests (X-Bus V1)

**Format:**

|         | Header byte | Data Byte 1          | X-Or-byte  |
|---------|-------------|----------------------|------------|
| Binary: | 1010 0001   | Locomotive address   | X-Or-byte  |
| Hex:    | 0xA1        | Locomotive address   | X-Or-byte  |
| Decimal:| 161         | Locomotive address   | X-Or-byte  |

**Description:**

14 speed steps was the only mode supported until X-Bus V2. Therefore no additional distinction byte (ModSel) was necessary. A locomotive inquiry with this instruction causes that the command station to respond only with the Locomotive information response (X-Bus V1) since the command station assumes that any device that issues this request does not understand the more advanced modes.

**Comments:**

Locomotive address is in the range 0 to 99.

**Response:**

The response is described in section 3.12.

#### 4.19.2 Locomotive information requests (X-Bus V1 and V2)

**Format:**

|         | Header byte | Data Byte 1          | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------------|-------------|------------|
| Binary: | 1010 0010   | Locomotive address   | ModSel      | X-Or-byte  |
| Hex:    | 0xA2        | Locomotive address   | ModSel      | X-Or-byte  |
| Decimal:| 162         | Locomotive address   | ModSel      | X-Or-byte  |

**Description:**

Starting with X-Bus V2 14,27, and 28 speed step modes are supported. To distinguish the speed step mode the ModSel byte is sent with each request. A command station that supports X-Bus 2.0 is designed to answer with the inclusive ModSel byte so that the network device can determine the speed step mode of the inquired locomotive.

**Comments:**

Locomotive address is in the range 0 to 99.

**Response:**

The response is described in section 3.13.

#### 4.19.3 Locomotive information requests (XpressNet only)

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|------------|
| Binary: | 1110 0011   | 0000 0000      | Address High | Address Low | X-Or-byte  |
| Hex:    | 0xE3        | 0x00           | AH           | AL          | X-Or-byte  |
| Decimal:| 227         | 0              | AH           | AL          | X-Or-byte  |

**Description:**

XpressNet provides a much richer response to locomotive information requests. To allow support for the full range of 10,000 locomotive addresses two bytes (AH/AL) are used to specify the address of the locomotive that the request is for. The locomotive address AH/AL is computed as described in section 3.15.

**Comments:**

It is possible to inquire the status of locomotives over the complete range of addressees from 0 to 9999.

**Response:**

The response is described in section 3.14.

#### 4.19.4 Function status request (XpressNet only)

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|------------|
| Binary: | 1110 0011   | 0000 0111      | Address High | Address Low | X-Or-byte  |
| Hex:    | 0xE3        | 0x07           | AH           | AL          | X-Or-byte  |
| Decimal:| 227         | 7              | AH           | AL          | X-Or-byte  |

**Description:**

XpressNet supports the concept of both momentary functions and on/off functions. The command station maintains the status of a function on whether it is momentary or on/off. It is the responsibility of the XpressNet device that is controlling the function to determine the length of a time a momentary function is on. This request is used to ask the command station for the current function status for a particular locomotive address.

To allow support for the full range of 10,000 locomotive addresses two bytes (AH/AL) are used to specify the address of the locomotive that the request is for. The locomotive address AH/AL is computed as described in section 3.15.

**Comments:**

None.

**Response:**

The current state of functions F0 to F12 as momentary or on/off, as described in section 3.16, is provided as a response to this request.

### 4.20 Locomotive operations request

#### 4.20.1 Locomotive operations (X-Bus V1)

**Format:**

|         | Header byte | Data Byte 1          | Data Byte 2             | Data Byte 3             | X-Or-byte  |
|---------|-------------|----------------------|-------------------------|-------------------------|------------|
| Binary: | 1011 0011   | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | X-Or-byte  |
| Hex:    | 0xB3        | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | X-Or-byte  |
| Decimal:| 179         | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | X-Or-byte  |

**Description:**

X-Bus V1 only supports 14 speed steps so does not need the (ModSel) byte to distinguish between different speed step modes.

Locomotive Data Byte 1 and locomotive Data Byte 2 are coded as described under 3.12.1.

**Comments:**

Locomotive address is in the range from 0 to 99.

**Response:**

None.

#### 4.20.2 Locomotive operations (X-Bus V2)

**Format:**

|         | Header byte | Data Byte 1          | Data Byte 2             | Data Byte 3             | Data Byte 4 | X-Or-byte  |
|---------|-------------|----------------------|-------------------------|-------------------------|-------------|------------|
| Binary: | 1011 0100   | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | ModSel      | X-Or-byte  |
| Hex:    | 0xB4        | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | ModSel      | X-Or-byte  |
| Decimal:| 180         | Locomotive address   | Locomotive Data Byte 1  | Locomotive Data Byte 2  | ModSel      | X-Or-byte  |

**Description:**

X-Bus V2 supports 14, 27 and 28 speed steps. The ModSel byte is used to indicate which speed step mode to use, so that the command station can modulate an appropriate track signal.

Locomotive Data Byte 1, locomotive Data Byte 2 and ModSel are coded as described under 3.13.1.

**Comments:**

Locomotive address is in the range 0 to 99

**Response:**

None.

#### 4.20.3 Locomotive speed and direction operations (XpressNet only)

XpressNet supports 14, 27, 28 and 128 speed step modes as well as the full 4 digit locomotive address range. To provide room for expansion, the ModSel byte was replaced by the Identification Byte. The speed and direction information for 14, 27 and 28 speed steps is coded as described in section 3.13.1. The speed and direction information for 128 speed steps is coded as described in section 3.14.1.

**Format - Speed and direction instruction for 14 speed steps:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0001 0000      | Address High | Address Low | R000 VVVV   | X-Or-byte  |
| Hex:    | 0xE4        | 0x10           | AH           | AL          | RV          | X-Or-byte  |
| Decimal:| 228         | 16             | AH           | AL          | RV          | X-Or-byte  |

**Format - Speed and direction instruction for 27 speed steps:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0001 0001      | Address High | Address Low | R00V VVVV   | X-Or-byte  |
| Hex:    | 0xE4        | 0x11           | AH           | AL          | RV          | X-Or-byte  |
| Decimal:| 228         | 17             | AH           | AL          | RV          | X-Or-byte  |

**Format - Speed and direction instruction for 28 speed steps:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0001 0010      | Address High | Address Low | R00V VVVV   | X-Or-byte  |
| Hex:    | 0xE4        | 0x12           | AH           | AL          | RV          | X-Or-byte  |
| Decimal:| 228         | 18             | AH           | AL          | RV          | X-Or-byte  |

**Format - Speed and direction instruction for 128 speed steps:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0001 0011      | Address High | Address Low | RVVV VVVV   | X-Or-byte  |
| Hex:    | 0xE4        | 0x13           | AH           | AL          | RV          | X-Or-byte  |
| Decimal:| 228         | 19             | AH           | AL          | RV          | X-Or-byte  |

**Description:**

Locomotive Speed and direction operations for XpressNet contain only the speed and direction instructions. Control of the functions is handled separately.

**Comments:**

Locomotives with addresses 0 to 9999 can be controlled. The locomotive address is specified in AH/AL as specified in section 3.15.

**Response:**

None.

#### 4.20.4 Function operation instructions (XpressNet only)

XpressNet supports all 13 NMRA DCC functions (F0-F12). The function instructions for a locomotive subdivide themselves in 3 different groups, which correspond to the three function types in a DCC packet. These groups are group 1 (F0 to F4), group 2 (F5 to F8) and group 3 (F9 to F12). The specific function group is specified in the identification data byte of the packet.

**Format - Function instruction group 1:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0000      | Address High | Address Low | 000F FFFF   | X-Or-byte  |
| Hex:    | 0xE4        | 0x20           | AH           | AL          | Group 1     | X-Or-byte  |
| Decimal:| 228         | 32             | AH           | AL          | Group 1     | X-Or-byte  |

**Format - Function instruction group 2:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0001      | Address High | Address Low | 0000 FFFF   | X-Or-byte  |
| Hex:    | 0xE4        | 0x21           | AH           | AL          | Group 2     | X-Or-byte  |
| Decimal:| 228         | 33             | AH           | AL          | Group 2     | X-Or-byte  |

**Format - Function instruction group 3:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0010      | Address High | Address Low | 0000 FFFF   | X-Or-byte  |
| Hex:    | 0xE4        | 0x22           | AH           | AL          | Group 3     | X-Or-byte  |
| Decimal:| 228         | 34             | AH           | AL          | Group 3     | X-Or-byte  |

**Description:**

Data Byte 3 indicated the current status of each function. If Fx=1, then is the function on, otherwise the function is off

- Group 1: 0 0 0 F0 F4 F3 F2 F1
- Group 2: 0 0 0 0 F8 F7 F6 F5
- Group 3: 0 0 0 0 F12 F11 F10 F9

**Comments:**

Locomotives with addresses 0 to 9999 can be controlled. The locomotive address is specified in AH/AL as specified in section 3.15.

**Response:**

None.

#### 4.20.5 Set function state (XpressNet only)

XpressNet allows functions F0-F12 to be specified as either monetary on constant on/off. Momentary functions are especially useful for sound control. The state of each function for each locomotive is stored in a database maintained by the command station (first implemented in LZ100V3). XpressNet devices can set or query the state of functions for each locomotive. It is up to the XpressNet device to determine the length of time that a momentary function is on. The function will remain on until switched off by the XpressNet device. The functions are grouped into three sets: group 1, group 2, and group 3.

**Format - Set Function state group 1:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0100      | Address High | Address Low | 000S SSSS   | X-Or-byte  |
| Hex:    | 0xE4        | 0x24           | AH           | AL          | Group 1     | X-Or-byte  |
| Decimal:| 228         | 36             | AH           | AL          | Group 1     | X-Or-byte  |

**Format - Set Function state group 2:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0101      | Address High | Address Low | 0000 SSSS   | X-Or-byte  |
| Hex:    | 0xE4        | 0x25           | AH           | AL          | Group 2     | X-Or-byte  |
| Decimal:| 228         | 37             | AH           | AL          | Group 2     | X-Or-byte  |

**Format - Set Function state group 3:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0010 0110      | Address High | Address Low | 0000 SSSS   | X-Or-byte  |
| Hex:    | 0xE4        | 0x26           | AH           | AL          | Group 3     | X-Or-byte  |
| Decimal:| 228         | 38             | AH           | AL          | Group 3     | X-Or-byte  |

**Description:**

The meanings for SSSS for each group follow:

Data Byte 3 indicated the current state of each function. If Sx=1, then the function is on/off. If Sx=0 then the function id momentary.

- Group 1: 0 0 0 S0 S4 S3 S2 S1
- Group 2: 0 0 0 0 S8 S7 S6 S5
- Group 3: 0 0 0 0 S12 S11 S10 S9

**Comments:**

Locomotives with addresses 0 to 9999 can be controlled. The locomotive address is specified in AH/AL as specified in section 3.15.

**Response:**

None.

### 4.21 Double Header operations (X-Bus V1 and V2)

#### 4.21.1 Establish Double Header

**Format:**

|         | Header byte | Identification | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|-------------|-------------|------------|
| Binary: | 1100 0011   | 0000 0101      | Address 1   | Address 2   | X-Or-byte  |
| Hex:    | 0xC3        | 0x05           | Address 1   | Address 2   | X-Or-byte  |
| Decimal:| 195         | 5              | Address 1   | Address 2   | X-Or-byte  |

**Description:**

The locomotives in Data Byte 1 and Data Byte 2 are joined in the command station to a Double Header. Once set up, speed and direction commands sent to one of the locomotive addresses are sent to both locomotives by the command station.

**Comments:**

Locomotives with addresses 1 to 99 can be placed in a double header using this request.

**Response:**

If this request is not successful, then the command station sends one of the error messages as described in section 3.20.

#### 4.21.2 Dissolve Double Header

**Format:**

|         | Header byte | Identification | Data Byte 1 | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|-------------|-------------|------------|
| Binary: | 1100 0011   | 0000 0100      | Address 1   | Address 2   | X-Or-byte  |
| Hex:    | 0xC3        | 0x04           | Address 1   | Address 2   | X-Or-byte  |
| Decimal:| 195         | 4              | Address 1   | Address 2   | X-Or-byte  |

**Description:**

This request instructs the command station to dissolve the Double Header of the locomotives as specified in Data Byte 1 and Data Byte 2.

**Comments:**

None.

**Response:**

If this request is not successful, then the command station sends one of the error messages as described in section 3.20.

### 4.22 Double Header operations (XpressNet only)

#### 4.22.1 Establish Double Header

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3  | Data Byte 4 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|--------------|-------------|------------|
| Binary: | 1110 0101   | 0100 0011      | Addr High 1  | Addr Low 1  | Addr High 2  | Addr Low 2  | X-Or-byte  |
| Hex:    | 0xE5        | 0x43           | AH1          | AL1         | AH 2         | AL2         | X-Or-byte  |
| Decimal:| 229         | 67             | AH1          | AL1         | AH 2         | AL2         | X-Or-byte  |

**Description:**

The locomotives specified in Data Bytes 1/2 and Data Bytes 3/4 are joined inside the command station as a Double Header. This means that speed and direction commands are sent to both locomotives by the command station whenever there is a change to the speed or direction of either locomotive.

Locomotives with addresses 0 to 9999 can be controlled. The locomotive address is specified in AH/AL as specified in section 3.15.

**Comments:**

The instruction replaces the old Double Header instructions, which is no longer supported by XpressNet or by command station versions starting with V3.

**Response:**

If Double Header creation is not successful, then the command station sends one of the error messages as described in section 3.21.

#### 4.22.2 Dissolve Double Header

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | Data Byte 4 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|-------------|------------|
| Binary: | 1110 0101   | 0100 0011      | Addr High 1  | Addr Low 1  | 0000 0000   | 0000 0000   | X-Or-byte  |
| Hex:    | 0xE5        | 0x43           | AH1          | AL1         | 0x00        | 0x00        | X-Or-byte  |
| Decimal:| 229         | 67             | AH1          | AL1         | 0x00        | 0x00        | X-Or-byte  |

**Description:**

The locomotive in Data Byte 1/2 is removed from the Double Header, in which it is merged. Thus also dissolves the Double Header in the command station.

This instruction is identical to the establish DH command. The command station recognizes that this command dissolves the DTR by a value of 0 in the second locomotive address.

Locomotives with addresses 0 to 9999 can be controlled. The locomotive address is specified in AH/AL as specified in section 3.15.

**Comments:**

The instruction replaces the old Double Header instructions, which is no longer supported by XpressNet or by command station versions starting with V3.

**Response:**

If dissolving the DH is not successful, then the command station sends one of the error messages as described in section 3.21.

### 4.23 Operations Mode programming (XpressNet only)

Operations Mode Programming (also referred to as Programming on Main) allows a decoder's CV to be read (future feature) or modified, while the locomotive is on the layout in normal operation. A programming track is not necessary in this case. However, the address of a decoder cannot be changed, since this changing the address is reserved for service mode.

Command stations, which do not support Operations Mode Programming, respond with an Instruction not supported by command station as described in 3.10.

In contrast to service mode programming which is limited to 256 CVs, Operations Mode programming supports the full range of CVs from CV1 to CV1024. XpressNet devices should not permit changes to the decoder's active locomotive address.

#### 4.23.1 Operations Mode Programming byte mode write request

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | Data Byte 4 | Data Byte 5 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|-------------|-------------|------------|
| Binary: | 1110 0110   | 0011 0000      | Address High | Address Low | 1110 11ÇC   | CCCC CCCC   | DDDD DDDD   | X-Or-byte  |
| Hex:    | 0xE6        | 0x30           | AH           | AL          | 0xEC + C    | CV          | D           | X-Or-byte  |
| Decimal:| 230         | 48             | AH           | AL          | 236 + C     | CV          | D           | X-Or-byte  |

**Description:**

Data Byte 1 and Data Byte 2 indicate the locomotive address of 1..9999, that is desired to be programmed.

The locomotive address AH/AL computes itself as indicated by 3.15.

Since the full range of CVs are supported (0..1023), 10 bits are necessary to specify the CV number. The upper 2 bits (MSBs) of the CV are contained in bits 1 and 0 of Data Byte 3. The remainder of the CV address (the 8 LSB´s) is located in Data Byte 4.

The CV address used in this request is as it appears on the DCC track packet which is one less than the value that the user generally refers to it as. Thus the decoders address (CV1) is transmitted as 00 00000000.

The new value of the CV is located in Data Byte 5.

**Comments:**

XpressNet devices should not permit changes to the decoder's active locomotive address.

**Response:**

None

#### 4.23.2 Operations Mode programming bit mode write request

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | Data Byte 4 | Data Byte 5 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|-------------|-------------|------------|
| Binary: | 1110 0110   | 0011 0000      | Address High | Address Low | 1111 10ÇC   | CCCC CCCC   | 1111 WBBB   | X-Or-byte  |
| Hex:    | 0xE6        | 0x30           | AH           | AL          | 0xE8 + CC   | CV          | Value/Bit   | X-Or-byte  |
| Decimal:| 230         | 48             | AH           | AL          | 232 + CC    | CV          | Value/Bit   | X-Or-byte  |

**Description:**

Data Byte 1 and Data Byte 2 indicate the address (1 to 9999) of the decoder to be programmed. The locomotive address AH/AL is as described in section 3.15.

Since the full range of CVs are supported (0 to 1023), 10 bits are necessary to specify the CV number. The upper 2 bits (MSBs) of the CV are contained in bits 1 and 0 of Data Byte 3. The remainder of the CV address (the 8 LSB's) is located in Data Byte 4.

The CV address (CC CCCCCCCC) used in this request is as it appears on the DCC track packet which is one less than he value that the user generally refers to it. Thus the decoders address (CV1) is transmitted as 00 00000000.

The bit to be set and its new value is located in Data Byte 5 as follows:

- W is the bit value 0 or 1.
- B2, B1, B0 give the position of the bit on in the CV (bit location 0 to bit location 7).

**Comments:**

XpressNet devices should not permit changes to the decoder's active locomotive address.

**Response:**

None

### 4.24 Multi-unit operation (XpressNet only)

#### 4.24.1 Add a locomotive to a multi-unit request

A locomotive can be added to a multi-unit (MTR) consist, if it is not currently contained in another consist currently maintained by the command station. If the command station maintains knowledge of consists and if this is a new consist (the locomotive address is the first locomotive to be added to a MTR), then a new consist entry is produced automatically by the command station.

When a locomotive is added to a consist its relative direction in relation to the consist can also be defined. This allows head to head, tail to tail or elephant style consists. Relative direction information is indicated by a bit in the identification data byte (R).

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0100 000R      | Address High | Address Low | MTR         | X-Or-byte  |
| Hex:    | 0xE4        | 0x40 + R       | AH           | AL          | MTR         | X-Or-byte  |
| Decimal:| 228         | 64 + R         | AH           | AL          | MTR         | X-Or-byte  |

**Description:**

**R:** R = 0 the locomotives direction is the same as the consists direction
      R=1 the locomotives direction is reversed to the direction of the consist

Data Byte 1 and Data Byte 2 indicate the locomotive address of 1..9999, which is to be inserted into the MTR. The locomotive address AH/AL is as specified in section 3.15.

**MTR:** This is the MTR or consist address and must be within the range of 1 to 99.

**Comments:**

By convention a locomotive cannot be inserted into a multi-unit, which has the same address which means that if AH has a value of 0 then AL can not equal MTR.

**Response:**

If adding the requested locomotive to a Multi-unit is not successful, then the command station sends one of the error messages as described in section 3.21.

#### 4.24.2 Remove a locomotive from a Multi-unit request

If the command station maintains a database of consists, a locomotive can only be removed from a multi-unit (MTR) consist that it is currently within. If the command station maintains knowledge of consists and the locomotive address is the only locomotive within the MTR/consist, then the consist entry is deleted automatically by the command station.

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|-------------|------------|
| Binary: | 1110 0100   | 0100 0010      | Address High | Address Low | MTR         | X-Or-byte  |
| Hex:    | 0xE4        | 0x42           | AH           | AL          | MTR         | X-Or-byte  |
| Decimal:| 228         | 66             | AH           | AL          | MTR         | X-Or-byte  |

**Description:**

Data Byte 1 and Data Byte 2 indicate the locomotive address of 1 to 9999, which is to be removed from the MTR. The locomotive address AH/AL is as specified in section 3.15.

**MTR:** Data byte 3 contains the MTR/consist address within the range of 1 to 99.

**Comments:**

None.

**Response:**

If removing the requested locomotive from a Multi-unit is not successful, then the command station sends one of the error messages as described in section 3.21.

### 4.25 Address search operations (XpressNet only)

With the introduction of support for multi-unit and extended stack handling, it became necessary that XpressNet devices have an ability to determine the identity of all the locomotives in a group. This is done to assist the development of an easy to use and understand operator interface.

#### 4.25.1 Address inquiry member of a Multi-unit request

This instruction requests the next or previous member in the multi-unit consist.

**Format:**

|         | Header byte | Identification | Data Byte 1 | Data Byte 2  | Data Byte 3 | X-Or-byte  |
|---------|-------------|----------------|-------------|--------------|-------------|------------|
| Binary: | 1110 0100   | 0000 00RR      | MTR         | Address High | Address Low | X-Or-byte  |
| Hex:    | 0xE4        | 0x01 + R       | MTR         | AH           | AL          | X-Or-byte  |
| Decimal:| 228         | 1 + R          | MTR         | AH           | AL          | X-Or-byte  |

**Description:**

In order to provide fast access to members of a multi-unit consist, an XpressNet device can ask for the next member or previous member of a multi-unit consist. This command works on any member of a multi-unit or the multi-unit address itself.

**Note:** currently command station with version 3.x only support the forward search.(next member)

- Identification = 0x01: (RR=01) means forward search (next member)
- Identification = 0x02: (RR=10) means backwards search (previous member)

The MTR consist base address for the search is placed in Data Byte 1 within the range of 1 to 99.

Data Byte 2 and Data Byte 3 indicate the locomotive address (1 to 9999) of a member of the group to be searched. The response to this request will identify the next or previous member of the group. The locomotive address AH/AL is as specified in section 3.15

If you don't know a current member of a Multi-Unit, set Adr High and Adr Low to 0x00 and you will get the first locomotive in the Multi-Unit specified by the value of MTR. In the future, if direction is reverse (R=1) you will get the last member in the Multi-Unit specified by the value of MTR.

**Comments:**

None.

**Response:**

The locomotive address that is a result of the search is sent as a response as described in section 3.17.

#### 4.25.2 Address inquiry Multi-unit request

This request asks the command station to respond with the next base address of a MTR, which follows the inquired MTR (forward search) and/or precedes the inquired MTR (backwards search).

**Format:**

|         | Header byte | Identification | Data Byte 1 | X-Or-byte  |
|---------|-------------|----------------|-------------|------------|
| Binary: | 1110 0010   | 0000 0RRR      | MTR         | X-Or-byte  |
| Hex:    | 0xE2        | 0x03 + R       | MTR         | X-Or-byte  |
| Decimal:| 226         | 3 + R          | MTR         | X-Or-byte  |

**Description:**

**Note:** currently version 3.x command stations only support the forward search.(next member)

- Identification = 0x03: (RRR=011) means forward search (next member)
- Identification = 0x04: (RRR=100) means backwards search (previous member)

The MTR specified to start the search is placed in Data Byte 1 within the range of 1 to 99.

If you don't know the address of any MTR, set MTR to 0x00 and the response will the first MTR address. For later implementations: If direction is reverse (R=1) you get the last MTR address.

**Comments:**

None.

**Response:**

The next multi unit address that is a result of the search is sent as a response as described in section 3.17.

#### 4.25.3 Address inquiry locomotive at command station stack request

This request returns the locomotive address, that is stored in the command station stack behind (forward search) and/or before (backwards search) the locomotive address specified in Data Byte 1 and Data Byte 2.

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|------------|
| Binary: | 1110 0011   | 0000 01RR      | Address High | Address Low | X-Or-byte  |
| Hex:    | 0xE3        | 0x05 + R       | AH           | AL          | X-Or-byte  |
| Decimal:| 227         | 5 + R          | AH           | AL          | X-Or-byte  |

**Description:**

**Note:** command stations version 3.x currently only supports the forward search (next member).

- Identification = 0x05: (RR=01) means forward search (next member)
- Identification = 0x06: (RR=10) means backwards search (previous member)

Data Byte 1 and Data Byte 2 indicate the locomotive address (1 to 9999) within the command station database. This request will identify the next or previous address in the command station database. The locomotive address AH/AL is as specified in section 3.15

In this case you do not know the first locomotive in the database, you can set Adr High and Adr Low to 0x00 to get the first (or last) member in the database.

In the stack the order of locomotive addresses is not by number. They are sorted by the time the addresses was first used by an XpressNet device.

**Comments:**

None.

**Response:**

The next locomotive address that is a result of the search is sent as a response as described in section 3.17.

### 4.26 Delete locomotive from command station stack request

This request returns the locomotive address specified in Data Byte 1 and Data Byte 2 be removed from the command station stack.

**Format:**

|         | Header byte | Identification | Data Byte 1  | Data Byte 2 | X-Or-byte  |
|---------|-------------|----------------|--------------|-------------|------------|
| Binary: | 1110 0011   | 0100 0100      | Address High | Address Low | X-Or-byte  |
| Hex:    | 0xE3        | 0x44           | AH           | AL          | X-Or-byte  |
| Decimal:| 227         | 68             | AH           | AL          | X-Or-byte  |

**Description:**

Data Byte 1 and Data Byte 2 indicate the locomotive address (1 to 9999) that is to be deleted from the command station stack. The locomotive address AH/AL is as specified in section 3.15

**Comments:**

The command station database contains information on each locomotive being controlled. Most command stations can only support a limited number of entries. When the database is full a member of the database must be removed before a new entry can be added. This problem is more frequently encountered with command stations with limited hardware that have limited space for locomotive address entries.

Before requesting that a specific entry be deleted, the XpressNet device should first determine that the entry is not being currently controlled by another XpressNet device. Otherwise the entry deleted would immediately be requested to be entered again.

**Response:**

None.
