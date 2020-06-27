<?xml version="1.0" encoding="utf-8"?>
<Document xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/RelicCore.ModProject">
	<Children xmlns:d2p1="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Data</Alias>
			<Children>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="Folder">
							<Children>
								<d2p1:anyType i:type="BurnFile">
									<BurnSettings i:nil="true" />
									<RelativeName>coh2_battlegrounds_annihilate.win</RelativeName>
								</d2p1:anyType>
								<d2p1:anyType i:type="BurnFile">
									<BurnSettings i:nil="true" />
									<RelativeName>coh2_battlegrounds_vp.win</RelativeName>
								</d2p1:anyType>
							</Children>
							<IsExpanded>true</IsExpanded>
							<Name>winconditions</Name>
						</d2p1:anyType>
					</Children>
					<IsExpanded>true</IsExpanded>
					<Name>game</Name>
				</d2p1:anyType>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="Folder">
							<Children>
								<d2p1:anyType i:type="Folder">
									<Children>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\shared_handler.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\shared_units.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\shared_util.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\client_companyui.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\shared_companyloader.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\session.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\api_ui.scar</RelativeName>
										</d2p1:anyType>
										<d2p1:anyType i:type="BurnFile">
											<BurnSettings i:nil="true" />
											<RelativeName>auxiliary_scripts\client_overrideui.scar</RelativeName>
										</d2p1:anyType>
									</Children>
									<IsExpanded>true</IsExpanded>
									<Name>auxiliary_scripts</Name>
								</d2p1:anyType>
								<d2p1:anyType i:type="BurnFile">
									<BurnSettings i:nil="true" />
									<RelativeName>coh2_battlegrounds_vp.scar</RelativeName>
								</d2p1:anyType>
								<d2p1:anyType i:type="BurnFile">
									<BurnSettings i:nil="true" />
									<RelativeName>coh2_battlegrounds_annihilate.scar</RelativeName>
								</d2p1:anyType>
							</Children>
							<IsExpanded>true</IsExpanded>
							<Name>winconditions</Name>
						</d2p1:anyType>
					</Children>
					<IsExpanded>true</IsExpanded>
					<Name>scar</Name>
				</d2p1:anyType>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="BurnIcons">
							<PackSize>1024</PackSize>
							<RelativeName>art</RelativeName>
						</d2p1:anyType>
					</Children>
					<IsExpanded>false</IsExpanded>
					<Name>ui</Name>
				</d2p1:anyType>
			</Children>
			<IsExpanded>true</IsExpanded>
		</d2p1:anyType>
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Info</Alias>
			<Children>
				<d2p1:anyType i:type="BurnModInfo">
					<Dependencies />
					<Description>This be something</Description>
					<Hidden>false</Hidden>
					<Name>Battlegrounds Wincondition</Name>
				</d2p1:anyType>
				<d2p1:anyType i:type="BurnFile">
					<BurnSettings i:type="GenericImageToGenericDDSBurnSettings">
						<AlphaEdge>false</AlphaEdge>
						<BlackBorder>false</BlackBorder>
						<CompressTextures>true</CompressTextures>
						<FlipImage>false</FlipImage>
						<ForceFormat>false</ForceFormat>
						<Metadata i:nil="true" />
						<MipDrop>0</MipDrop>
						<MipMap>false</MipMap>
						<PreferredFormat>Default</PreferredFormat>
						<RescaleNonPowerTwo>false</RescaleNonPowerTwo>
						<TexSharpen>false</TexSharpen>
					</BurnSettings>
					<RelativeName>coh2_battlegrounds_wincondition_preview.tga</RelativeName>
				</d2p1:anyType>
			</Children>
			<IsExpanded>true</IsExpanded>
		</d2p1:anyType>
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Locale</Alias>
			<Children>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="BurnFile">
							<BurnSettings i:nil="true" />
							<RelativeName>locale\english\english.ucs</RelativeName>
						</d2p1:anyType>
					</Children>
					<IsExpanded>false</IsExpanded>
					<Name>english</Name>
				</d2p1:anyType>
			</Children>
			<IsExpanded>false</IsExpanded>
		</d2p1:anyType>
	</Children>
	<Guid>6a0a13b8-9555-402c-a75b-85dc30f5cb04</Guid>
	<IsExpanded>false</IsExpanded>
	<Type>WinConditionPack</Type>
</Document>