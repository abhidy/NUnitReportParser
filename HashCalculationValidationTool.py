import sys
import re
import array
import time
from datetime import datetime
import serial
import os.path
import random
from os import path
import argparse, textwrap

parser = argparse.ArgumentParser(
description='Test Application to compare the generated Hash between PC Toool and Firmware Hash Library',
usage='use "python HashCalculationValidationTool.py --help" for more information')

parser.add_argument('pcTool',help="PC Tool executor complete Path",type=str)
parser.add_argument('signTool',help="Sign Tool executor complete Path",type=str)
parser.add_argument('privateKey',help="PrivateKey",type=str)
parser.add_argument("logOutput",help="Log File containing the comparison results",type=str)

parser.add_argument(
	'-mp',
	action="store",
	dest="comPort1",
	type=str,
	help='Com Port for first Device Under Test',
	default = "NULL"
	)
	
parser.add_argument(
	'-ap',
	action="store",
	dest="comPort2",
	type=str,
	help='Com Port for Additional Device Under Test',
	default = "NULL"
	)

parser.add_argument(
	'-baseDir',
	action="store",
	dest="baseDir",
	type=str,
	help='Base Dir To Generate Bin and Store Results.',
	default="."
	)

parser.add_argument(
	'-stop',
	action="store",
	dest="stopFileName",
	type=str,
	help='Base Dir To Generate Bin and Store Results.',
	default="Stop.txt"
	)

parser.add_argument(
	'-samples',
	action="store",
	dest="totalSamples",
	type=int,
	help='Total Samples to generated',
	default=1
	)
	
parser.add_argument(
	'-infiniteLoop',
	action="store",
	dest="infiniteLoopedExecution",
	type=int,
	help='Total Samples to generated',
	default=0
	)	

parser.add_argument(
	'-maxSize',
	action="store",
	dest="maxSizeofBinData",
	type=int,
	help='Maximum Size of Bin Data',
	default=262144
	)	

parser.add_argument(
	'-minSize',
	action="store",
	dest="minSizeofBinData",
	type=int,
	help='Minimum Size of Bin Data',
	default=16
	)	

ResearchToolBaseDir="ResearchTool"
SignGeneratorToolVerboseOption=" -v sample\pubkey.hex"
ResearchToolSlnCmd="msbuild "+ResearchToolBaseDir+"\CodeSigning.sln"
ResearchToolSlnCleanCmd=ResearchToolSlnCmd+" /t:clean >nul"
ResearchToolSlnBuildCmd=ResearchToolSlnCmd+">nul"
ResearchToolExeLog="1.txt"
ResearchToolExe=ResearchToolBaseDir+"\Debug\ZBPECC256Test.exe > "+ResearchToolExeLog+" 2>&1"
ResearchToolGeneratedArrayFileName=ResearchToolBaseDir+"\\ZBPECC256Test\\Arrays.c"
	
args = parser.parse_args()

class UtilityFunctions:
	@staticmethod
	def DeleteFile(filePath):
		if os.path.exists(filePath):
		  os.remove(filePath)

	@staticmethod
	def ReadPartOfString(arrayName,inputData,delimiterStart,delimiterEnd):
		splittedData = inputData.split(":")
		arrayContent = [ "0x", "0x", "0x", "0x", "0x", "0x", "0x", "0x" ];
		selectedSplittedData = ["", "", "", "", "", "", "", "","", "", "", "", "", "", "", "","", "", "", "", "", "", "", "","", "", "", "", "", "", "", ""]
		
		counter = 0
		counterArray = 0
		for value in splittedData:
			if counter >= delimiterStart and counter <= delimiterEnd:
				selectedSplittedData[counterArray]= value
				counterArray = counterArray+1
			counter = counter + 1

		counter = 0
		counterArray = 0			
		for value in selectedSplittedData:
			arrayContent[7- counter/4] += value
			counter = counter + 1
		
		FormettedString = arrayName + " = {"
		for value in arrayContent:
				FormettedString += value + ","
		FormettedString += "};"
		FormettedString = FormettedString.replace(",};", "};")
		return FormettedString
	
	@staticmethod
	def WriteBinaryContentToFile(inputFilePath,generatedArray):
		UtilityFunctions.DeleteFile(inputFilePath)
		fileRef = open(inputFilePath,"wb")
		binaryFormatArray = bytearray(generatedArray)
		fileRef.write(binaryFormatArray)
		fileRef.close()

	@staticmethod
	def ReadContentofFile(inputFilePath):
		inFileRef = open(inputFilePath, "r")
		inFileContent = inFileRef.read()
		inFileRef.close()
		return inFileContent

	@staticmethod
	def ReadContentofFileFromBinary(inputFilePath):
		inFileRef = open(inputFilePath, "rb")
		inFileContent = inFileRef.read()
		binaryArray = bytearray(inFileContent)
		inFileRef.close()
		return binaryArray

	@staticmethod
	def GetCurrentDateTimeString():
		now = datetime.now()
		baseFileName = now.strftime("%Y%m%d-%H%M%S%f")
		return baseFileName

	@staticmethod
	def GeneratedSlicedArray(array,length):
		retout = [array[i:i+length] for i in range(0, len(array), length)]
		return retout

	@staticmethod
	def GenerateFilePathforReg(strin):
		strout = strin.replace("\\","\\\\")
		return strout

	@staticmethod
	def ReadSerial(device, timeout=5):
		recv = ''
		data = ""
		cdata = ""
		Endtime = time.time() + timeout
		state = 0
		while True and (time.time() < Endtime):
			recv = device.read(1)
			if state == 0 and recv == "[":
				cdata += recv
				state = 1
			elif state == 1 and recv != "]" :
				cdata += recv
			elif state == 1 and recv == "]":
				cdata += recv
				data = cdata.lstrip("\r").lstrip("\n").rstrip("\r").rstrip("\n")
				ValidationSuite.WriteToLog("ReadSerial Data: "+data)
				break

		return data

	@staticmethod
	def SendCommand(device, command, timeout=0.1):
		command = command+"\r\n"
		ValidationSuite.WriteToLog("Sending Command: "+command)
		device.write("[]")
		time.sleep(timeout)
		device.flushInput()
		device.write(command)

	@staticmethod
	def WaitForResponse(hSerial, response, timeout=10):
		ValidationSuite.WriteToLog("WaitForResponse: "+response)
		for i in range(timeout):
			dataRead = UtilityFunctions.ReadSerial(hSerial)
			if response in dataRead:
					return True,dataRead
		ValidationSuite.WriteToLog("Response not Recvd !!")
		return False,dataRead
	
class DeviceExecutor:
	__comPort = ""
	__serialPortRef = ""
	__serialCommandDataLength=16
	__receivedHashValue = ""

	def __init__(self,comPort):
		self.__comPort = comPort

	def IsDeviceExist(self):
		try:
			self.__serialPortRef = serial.Serial(
			self.__comPort,\
			baudrate=115200,\
			bytesize=8,\
			parity='N',\
			stopbits=1,\
			timeout=10,\
			rtscts=False)
		except Exception as e:
			ValidationSuite.WriteToLog("@@Error@@ " + self.__comPort + " Port is eiter not accessible or not existing!")
			sys.exit(1)

	def IsDeviceReady(self):
		ValidationSuite.WriteToLog("writing reset Command")
		UtilityFunctions.SendCommand(self.__serialPortRef,"[TH,Reset]",5)
		UtilityFunctions.WaitForResponse(self.__serialPortRef,"[TH,Ready,0]")

	def SendBinaryData(self,binFilePath):
		self.__receivedHashValue = ""
		byteArray = UtilityFunctions.ReadContentofFileFromBinary(binFilePath)
		slicedArray = UtilityFunctions.GeneratedSlicedArray(byteArray,self.__serialCommandDataLength)

		#[S_Mdc2, HashInit]
		UtilityFunctions.SendCommand(self.__serialPortRef,"[S_Mdc2,HashInit]")
		#wait and result response from HashInit		
		#[S_Mdc2,HashInit,0]
		UtilityFunctions.WaitForResponse(self.__serialPortRef,"[S_Mdc2,HashInit,0]")

		#command Format
		for row in slicedArray:
			commandData = ""
			for data in row:
				commandData = commandData + '{:02X}'.format(data)		

			#[S_Mdc2, DataToHash, <databytes>]				
			UtilityFunctions.SendCommand(self.__serialPortRef,"[S_Mdc2,DataToHash,"+commandData+"]")
			#wait and result response from HashData
			#[S_Mdc2,DataToHash,0]
			UtilityFunctions.WaitForResponse(self.__serialPortRef,"[S_Mdc2,DataToHash,0]")

		#[S_Mdc2, HashFinal]			
		UtilityFunctions.SendCommand(self.__serialPortRef,"[S_Mdc2,HashFinal]")
		#wait and result response from HashFinal
		#[S_Mdc2,HashOutput:,0x6C,0x76,0xB3,0xFF,0x81,0x20,0x2C,0x6E,0xA9,0x37,0x56,0xCA,0x99,0x3D,0x5E,0x8,0xEA,0x21,0xDE,0xFC,0x95,0x86,0x46,0xEF,0xF4,0x6A,0x1,0xD8,0x2F,0x79,0x53,0xC3]
		returnHashDataCommand = UtilityFunctions.WaitForResponse(self.__serialPortRef,"[S_Mdc2,HashOutput")	
		#[S_Mdc2,HashFinal,0]
		UtilityFunctions.WaitForResponse(self.__serialPortRef,"[S_Mdc2,HashFinal,0]")
		regexpr = "\[S_Mdc2,HashOutput((,[0-9A-F]+){32})\]"
		match = re.search(regexpr,returnHashDataCommand[1])
		if match:
			self.__receivedHashValue = match.group(1)
			self.__receivedHashValue = self.__receivedHashValue.replace(",","")
			self.__receivedHashValue = self.__receivedHashValue.upper()
			
		errorStatus = False
		return errorStatus

	def CloseDeviceRef(self):
		self.__serialPortRef.close()

	def GetReceivedHash(self):
		return self.__receivedHashValue

class SignToolExecutor:

	__filePrefix = ""
	__inputHashFileName = ""
	__outputSignFileName = ""
	__outputArrayFileName = ""	
	__inputPriateKeyFileName = ""
	__logFileName = ""
	__inputFileSize = 0
	__isErrorNotified = False
	__receivedHashValue = ""

	def __init__(self,pcToolPath,privKeyPath):
		self.__pcToolPath = pcToolPath
		self.__inputPriateKeyFileName = privKeyPath
		

	def DeleteTempFiles(self):
		UtilityFunctions.DeleteFile(self.__outputSighFileName)
		UtilityFunctions.DeleteFile(self.__logFileName)
		self.__isErrorNotified = False

	def GenerateFileNames(self,hashFilePath):
		self.__receivedSignValue = ""
		self.__isErrorNotified = False		
		self.__inputHashFileName = hashFilePath
		self.__outputSignFileName = hashFilePath.replace(".hash",".sign")
		UtilityFunctions.DeleteFile(self.__outputSignFileName)			
		self.__logFileName = hashFilePath.replace(".hash",".signtoollog")
		
		
		self.__outputArrayFileName = ResearchToolGeneratedArrayFileName

	def ExecuteCommand(self):
		command = self.__pcToolPath + 	\
		" -o "+ self.__outputSignFileName + 	\
		" -i " + self.__inputHashFileName +	\
		" -k " + self.__inputPriateKeyFileName +	\
		SignGeneratorToolVerboseOption+	\
		" >> " + self.__logFileName + " 2>&1"
		os.system(command)

	def GeneratedArrayResults(self):
		UtilityFunctions.DeleteFile(self.__outputArrayFileName)			
		HashFileContent = UtilityFunctions.ReadContentofFile(self.__inputHashFileName)
		SignFileContent = UtilityFunctions.ReadContentofFile(self.__outputSignFileName)
		
		HashArray = UtilityFunctions.ReadPartOfString("uint32_t hash[8]",HashFileContent,0,31)
		SigGen_R = UtilityFunctions.ReadPartOfString("uint32_t s_r[8]",SignFileContent,0,31)		
		SigGen_S = UtilityFunctions.ReadPartOfString("uint32_t s_s[8]",SignFileContent,32,63)
		
		ValidationSuite.WriteToLog(HashArray)
		ValidationSuite.WriteToLog(SigGen_R)
		ValidationSuite.WriteToLog(SigGen_S)
		
		fileRef = open(self.__outputArrayFileName,"w")
		fileRef.write("\n#include <stdint.h>\n")
		fileRef.write("\n/*"+self.__inputHashFileName+"*/")
		fileRef.write("\n"+HashArray)
		fileRef.write("\n"+SigGen_R)		
		fileRef.write("\n"+SigGen_S)				
		fileRef.flush()
		fileRef.close()
		
		UtilityFunctions.DeleteFile(ResearchToolExeLog)	
		os.system(ResearchToolSlnCleanCmd)
		os.system(ResearchToolSlnBuildCmd)
		os.system(ResearchToolExe)
		UtilityFunctions.DeleteFile(self.__outputArrayFileName)			
		logFileContent = UtilityFunctions.ReadContentofFile(ResearchToolExeLog)
		
		ValidationSuite.WriteToLog("====")
		ValidationSuite.WriteToLog(logFileContent)
		ValidationSuite.WriteToLog("====")
		
		if not re.search("Check if Signature is OK:Signature OK",logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in Signature Verification")
			return True
			
		else:
			ValidationSuite.WriteToLog("@@Success@@ Identified in Signature Verification")		
			return True			
		
	def ValidateGeneratedToolResults(self):
		self.__isErrorNotified = False
		logFileContent = UtilityFunctions.ReadContentofFile(self.__logFileName)
		
		#cheking input file mentioning
		reportedInputFileCheckStr = "Hash File Name: "+UtilityFunctions.GenerateFilePathforReg(self.__inputHashFileName)
		reportedOutputFileName = "Output file name containing signature: "+UtilityFunctions.GenerateFilePathforReg(self.__outputSignFileName)
		reportedHashWrittenFileName = "Signature written to file: "+UtilityFunctions.GenerateFilePathforReg(self.__outputSignFileName)

		if not re.search(reportedInputFileCheckStr,logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in "+self.__inputHashFileName)
			self.__isErrorNotified = True

		if not re.search(reportedOutputFileName,logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in "+self.__outputSignFileName)
			self.__isErrorNotified = True			

		if not re.search(reportedHashWrittenFileName,logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in Generated file "+self.__outputSignFileName)			
			self.__isErrorNotified = True
		else:
			outFileContent = UtilityFunctions.ReadContentofFile(self.__outputSignFileName)

			reportedHashCheckStr = outFileContent
			ValidationSuite.WriteToLog("Generated Signature = "+outFileContent)

#			if not re.search(reportedInputFileSizeCheckStr,logFileContent):
#				ValidationSuite.WriteToLog("@@Error@@ Identified in "+hexFileSizeString)
#				self.__isErrorNotified = True

			if not re.search(reportedHashCheckStr,logFileContent):
				ValidationSuite.WriteToLog("@@Error@@ Identified in "+reportedHashCheckStr)
				self.__isErrorNotified = True

		#ValidationSuite.WriteToLog("@@Error@@="+'{0}'.format(self.__isErrorNotified))
		return self.__isErrorNotified

	def GetReceivedHash(self):
		return self.__receivedHashValue		
		
class PCToolExecutor:

	__filePrefix = ""
	__inputFileName = ""
	__outputFileName = ""
	__logFileName = ""
	__inputFileSize = 0
	__isErrorNotified = False
	__receivedHashValue = ""

	def __init__(self,pcToolPath):
		self.__pcToolPath = pcToolPath

	def DeleteTempFiles(self):
		#UtilityFunctions.DeleteFile(self.__inputFileName)
		#UtilityFunctions.DeleteFile(self.__outputFileName)
		#UtilityFunctions.DeleteFile(self.__logFileName)
		self.__isErrorNotified = False

	def GenerateRandomBinFile(self,minSize,maxSize):
		self.__inputFileSize =random.randint(minSize,maxSize)
		ValidationSuite.WriteToLog("=BinFile Size="+'0x{:0x}'.format(self.__inputFileSize)+"")

		generatedArray = []
		for x in range(self.__inputFileSize):
			generatedArray.append(random.randint(0,255))

		UtilityFunctions.WriteBinaryContentToFile(self.__inputFileName,generatedArray)

	def GenerateFileNames(self,basePath):
		self.__receivedHashValue = ""
		self.__isErrorNotified = False		
		self.__filePrefix = UtilityFunctions.GetCurrentDateTimeString()
		ValidationSuite.WriteToLog("\n=====FilePrefix="+self.__filePrefix+"=============")
		self.__inputFileName = basePath + "\\" +self.__filePrefix + ".bin"
		self.__outputFileName = basePath + "\\" +self.__filePrefix + ".hash"
		self.__logFileName = basePath + "\\" +self.__filePrefix + ".toollog"

	def GetInputFileName(self):
		return self.__inputFileName

	def GetOutputFileName(self):
		return self.__outputFileName

	def GetLogFileName(self):
		return self.__logFileName

	def GetFilePrefix(self):
		return self.__filePrefix

	def ExecuteCommand(self):
		command = self.__pcToolPath + 	\
		" -o "+ self.__outputFileName + 	\
		" -i " + self.__inputFileName +	\
		" > " + self.__logFileName + " 2>&1"
		os.system(command)


	def ValidateGeneratedToolResults(self):
		self.__isErrorNotified = False
		logFileContent = UtilityFunctions.ReadContentofFile(self.__logFileName)

		#cheking input file mentioning
		reportedInputFileCheckStr = "File to be hashed: "+UtilityFunctions.GenerateFilePathforReg(self.__inputFileName)
		reportedOutputFileName = "File containing the hash value: "+UtilityFunctions.GenerateFilePathforReg(self.__outputFileName)
		reportedHashWrittenFileName = "Success: Hash written to file: "+UtilityFunctions.GenerateFilePathforReg(self.__outputFileName)
		hexFileSizeString = "File size:"+'0x{:0x}'.format(self.__inputFileSize)
		reportedInputFileSizeCheckStr = hexFileSizeString
		reportedErrorCheckStr = "Error: Size of input file " + self.__inputFileName + " is greater than 0x40000"
		reportedErrorCheckStrAltPath = UtilityFunctions.GenerateFilePathforReg(reportedErrorCheckStr)

		if not re.search(reportedInputFileCheckStr,logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in "+self.__inputFileName)
			self.__isErrorNotified = True

		if not re.search(reportedOutputFileName,logFileContent):
			ValidationSuite.WriteToLog("@@Error@@ Identified in "+self.__outputFileName)
			self.__isErrorNotified = True			

		if not re.search(reportedHashWrittenFileName,logFileContent):

			if not re.search(reportedErrorCheckStrAltPath,logFileContent):
				ValidationSuite.WriteToLog("@@Error@@ Indicated "+reportedErrorCheckStr)
				self.__isErrorNotified = True
				
			self.__isErrorNotified = True
		else:
			outFileContent = UtilityFunctions.ReadContentofFile(self.__outputFileName)

			reportedHashCheckStr = "Hash value: "+outFileContent
			
			ValidationSuite.WriteToLog("Generated Hash = "+outFileContent)

			if not re.search(reportedInputFileSizeCheckStr,logFileContent):
				ValidationSuite.WriteToLog("@@Error@@ Identified in "+hexFileSizeString)
				self.__isErrorNotified = True

			self.__receivedHashValue = outFileContent.replace(":","")
			self.__receivedHashValue = self.__receivedHashValue.upper()
			if not re.search(reportedHashCheckStr,logFileContent):
				ValidationSuite.WriteToLog("@@Error@@ Identified in "+reportedHashCheckStr)
				self.__isErrorNotified = True

		#ValidationSuite.WriteToLog("@@Error@@="+'{0}'.format(self.__isErrorNotified))
		
		return self.__isErrorNotified

	def GetReceivedHash(self):
		return self.__receivedHashValue

class ValidationSuite:
	__pcToolPath = ""
	__signToolPath = ""
	__privKeyPath = ""	
	__baseDirPath = ""
	__totalSampleRequired = 0
	__fileRef = ""
	__currentPcToolRef = ""
	__currentSignToolRef = ""
	__currentDevice1Ref = ""
	__currentDevice2Ref = ""

	def __init__(self,pcToolPath,signToolPath,privKeyPath,baseDirPath,totalSampleRequired):
		self.__pcToolPath = pcToolPath
		self.__signToolPath = signToolPath	
		self.__privKeyPath = privKeyPath
		
		self.__baseDirPath = baseDirPath
		self.__totalSampleRequired = totalSampleRequired

	def IsPcToolExist(self):
		return path.isfile(self.__pcToolPath) and path.exists(self.__pcToolPath)

	def IsSignToolExist(self):
		return path.isfile(self.__signToolPath) and path.exists(self.__signToolPath)		

	def GetPCToolPath(self):
		return self.__pcToolPath

	def GetSignToolPath(self):
		return self.__signToolPath		

	def GetRequestedSampleCount(self):
		return self.__totalSampleRequired

	def ClearTempFiles(self):
		self.__currentPcToolRef.DeleteTempFiles()

	def CreateAndValidatePCToolReference(self,minSize,maxSize):
		errorStatus = False
		self.__currentPcToolRef = PCToolExecutor(self.__pcToolPath)
		self.__currentPcToolRef.GenerateFileNames(os.path.abspath(self.__baseDirPath))
		self.__currentPcToolRef.GenerateRandomBinFile(minSize,maxSize)
		self.__currentPcToolRef.ExecuteCommand()
		errorStatus = self.__currentPcToolRef.ValidateGeneratedToolResults()
		return errorStatus
		
	def CreateAndValidateSignToolReference(self):
		errorStatus = False
		self.__currentSignToolRef = SignToolExecutor(self.__signToolPath,self.__privKeyPath)
		self.__currentSignToolRef.GenerateFileNames(self.__currentPcToolRef.GetOutputFileName())
		self.__currentSignToolRef.ExecuteCommand()
		errorStatus = self.__currentSignToolRef.ValidateGeneratedToolResults()
		self.__currentSignToolRef.GeneratedArrayResults()
		return errorStatus		

	def CreateDeviceReference(self,comPort,devRef):
		errorStatus = False	
		devRef = DeviceExecutor(comPort)
		devRef.IsDeviceExist()		
		devRef.IsDeviceReady()
		return errorStatus

	def CreateDeviceReferences(self,comPort1,comPort2):	
		errorStatus = False		
		#errorStatus = self.CreateDeviceReference(comPort1,self.__currentDevice1Ref)
		self.__currentDevice1Ref = DeviceExecutor(comPort1)
		self.__currentDevice1Ref.IsDeviceExist()
		self.__currentDevice1Ref.IsDeviceReady()

		if comPort2!="NULL":
			#errorStatus = self.CreateDeviceReference(comPort2,self.__currentDevice2Ref)
			self.__currentDevice2Ref = DeviceExecutor(comPort2)
			self.__currentDevice2Ref.IsDeviceExist()
			self.__currentDevice2Ref.IsDeviceReady()
		return errorStatus	

	def CloseDeviceReferences(self):		
		self.__currentDevice1Ref.CloseDeviceRef()

		if self.__currentDevice2Ref!="":
			self.__currentDevice2Ref.CloseDeviceRef()

	def GetAndValidateDevice1Reference(self):
		errorStatus = False	
		errorStatus = self.__currentDevice1Ref.SendBinaryData(self.__currentPcToolRef.GetInputFileName())

		if errorStatus!=True:		
			if self.__currentDevice2Ref!="":
				errorStatus = self.__currentDevice2Ref.SendBinaryData(self.__currentPcToolRef.GetInputFileName())

		return errorStatus
		
	def CreateAndValidateSamples(self):
		retVal = self.CreateAndValidatePCToolReference(
					args.minSizeofBinData,\
					args.maxSizeofBinData) 
		if retVal != True:
			retVal = self.CreateAndValidateSignToolReference()
			if retVal != True:
				self.WriteToLog("@@Success@@ CreateAndValidateSignToolReference")
			else:
				self.WriteToLog("@@Error@@ CreateAndValidateSignToolReference")
			#retVal = self.GetAndValidateDevice1Reference()
			#if retVal != True:
			#	retVal = self.CompareReceivedHash()
			#else:
			#	self.WriteToLog("@@Error@@ GetAndValidateDevice1Reference")
			ValidationSuite.WriteToLog("@@Success@@ CreateAndValidatePCToolReference")
		else:
			ValidationSuite.WriteToLog("@@Error@@ CreateAndValidatePCToolReference")
			
		if retVal != True:
			self.ClearTempFiles()

	def CompareReceivedHash(self):
		errorStatus = False	
		GetReceivedHashFromTool = self.__currentPcToolRef.GetReceivedHash()
		GetReceivedHashFromDev1 = self.__currentDevice1Ref.GetReceivedHash()
		GetReceivedHashFromDev2 = ""

		if self.__currentDevice2Ref!="":
			GetReceivedHashFromDev1 = self.__currentDevice2Ref.GetReceivedHash()

		ValidationSuite.WriteToLog("Tool Hash:"+GetReceivedHashFromTool)
		ValidationSuite.WriteToLog("Dev1 Hash:"+GetReceivedHashFromDev1)

		if self.__currentDevice2Ref!="":
			ValidationSuite.WriteToLog("Dev2 Hash:"+GetReceivedHashFromDev2)

		if GetReceivedHashFromTool == GetReceivedHashFromDev1:

			if self.__currentDevice2Ref!="":
				if GetReceivedHashFromDev1 == GetReceivedHashFromDev2:
					ValidationSuite.WriteToLog("@@Success@@ HashTool = HashDev1  = HashDev2")
				else:
					ValidationSuite.WriteToLog("@@Error@@ HashTool = HashDev1 !=HashDev2")
					errorStatus = True
			else:
					ValidationSuite.WriteToLog("@@Success@@ HashTool  = HashDev1")
		else:
			if self.__currentDevice2Ref!="":
				if GetReceivedHashFromDev1 == GetReceivedHashFromDev2:
					ValidationSuite.WriteToLog("@@Success@@ HashTool != HashDev1 = HashDev2")
				else:
					ValidationSuite.WriteToLog("@@Error@@ HashTool != HashDev1 !=HashDev2")
			else:
				ValidationSuite.WriteToLog("@@Error@@ HashTool != HashDev1")
			
			errorStatus = True			

		return errorStatus

	@staticmethod
	def CheckForBreak(fileName):
		CheckForBreakExitStatus = False
		if os.path.exists(fileName):
			CheckForBreakExitStatus =  True

		return CheckForBreakExitStatus

	@staticmethod
	def CreateLogHandle(logFilePath):
		UtilityFunctions.DeleteFile(logFilePath)
		ValidationSuite.__fileRef = open(logFilePath,"w")
		baseFileName = UtilityFunctions.GetCurrentDateTimeString()
		ValidationSuite.__fileRef.write("File Created ["+baseFileName+"]")

	@staticmethod
	def WriteToLog(content):
		ValidationSuite.__fileRef.write("\n"+content)
		ValidationSuite.__fileRef.flush()

	@staticmethod
	def CloseLogHandle():
		ValidationSuite.__fileRef.close()

def main():

	validationSuite = ValidationSuite(args.pcTool,args.signTool,args.privateKey,args.baseDir,args.totalSamples)
	if validationSuite.IsPcToolExist() == False :
		print ("@Error@@ Following Tool Does Not exist!")
		print ("  "+validationSuite.GetPCToolPath())
		sys.exit(1)

	ValidationSuite.CreateLogHandle(args.logOutput)
	#validationSuite.CreateDeviceReferences(args.comPort1,args.comPort2)

	if (args.infiniteLoopedExecution == 1):
		UtilityFunctions.DeleteFile(args.stopFileName)
		while ValidationSuite.CheckForBreak(args.stopFileName)!=True:
			validationSuite.CreateAndValidateSamples()

	else:
		for x in range(validationSuite.GetRequestedSampleCount()):			
			validationSuite.CreateAndValidateSamples()

	#validationSuite.CloseDeviceReferences()
	ValidationSuite.CloseLogHandle()

if __name__== "__main__":
	main()
