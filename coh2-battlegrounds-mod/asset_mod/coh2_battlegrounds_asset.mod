<?xml version="1.0" encoding="utf-8"?>
<Document xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/RelicCore.ModProject">
	<Children xmlns:d2p1="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Attrib</Alias>
			<Children>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="BurnAttributes">
							<RelativeName>coh2_battlegrounds_asset_attrib.xml</RelativeName>
						</d2p1:anyType>
					</Children>
					<IsExpanded>false</IsExpanded>
					<Name>attrib</Name>
				</d2p1:anyType>
			</Children>
			<IsExpanded>false</IsExpanded>
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
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Info</Alias>
			<Children>
				<d2p1:anyType i:type="BurnModInfo">
					<Dependencies />
					<Description>Some other dope description</Description>
					<Hidden>false</Hidden>
					<Name>Battlegrounds Asset</Name>
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
					<RelativeName>coh2_battlegrounds_asset_preview.tga</RelativeName>
				</d2p1:anyType>
			</Children>
			<IsExpanded>false</IsExpanded>
		</d2p1:anyType>
		<d2p1:anyType i:type="TableOfContents">
			<Alias>Data</Alias>
			<Children>
				<d2p1:anyType i:type="Folder">
					<Children>
						<d2p1:anyType i:type="Folder">
							<Children>
								<d2p1:anyType i:type="Folder">
									<Children>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.bdf</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\grass\blades</RelativeName>
												</d2p1:anyType>
												<d2p1:anyType i:type="Folder">
													<Children>
														<d2p1:anyType i:type="BurnFolder">
															<BurnSettings>
																<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
																	<AlphaEdge>false</AlphaEdge>
																	<BlackBorder>false</BlackBorder>
																	<CompressTextures>true</CompressTextures>
																	<FlipImage>true</FlipImage>
																	<ForceFormat>false</ForceFormat>
																	<Metadata i:type="BurnFolderMetadata">
																		<Exclude />
																		<Include />
																	</Metadata>
																	<MipDrop>0</MipDrop>
																	<MipMap>true</MipMap>
																	<PreferredFormat>Default</PreferredFormat>
																	<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
																	<TexSharpen>false</TexSharpen>
																	<MixInputs></MixInputs>
																	<MixInputsDefaults></MixInputsDefaults>
																</d2p1:anyType>
															</BurnSettings>
															<Hint>Default</Hint>
															<RelativeName>art\grass\blades\textures</RelativeName>
														</d2p1:anyType>
													</Children>
													<IsExpanded>false</IsExpanded>
													<Name>textures</Name>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>blades</Name>
										</d2p1:anyType>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.gdf</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\grass\types</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>types</Name>
										</d2p1:anyType>
									</Children>
									<IsExpanded>false</IsExpanded>
									<Name>grass</Name>
								</d2p1:anyType>
								<d2p1:anyType i:type="Folder">
									<Children>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.aps</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\presets\atmosphere</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>atmosphere</Name>
										</d2p1:anyType>
									</Children>
									<IsExpanded>false</IsExpanded>
									<Name>presets</Name>
								</d2p1:anyType>
								<d2p1:anyType i:type="Folder">
									<Children>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>false</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*_dif.tga</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>DXT5</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs>(,,r)(,,g)(,,b)(_dif,_spc,r)</MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>false</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*_nrm.tga</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>DXT5</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs>(,,r)(,,g)(,,b)(_nrm,_gls,r)</MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.terrainmaterial</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\ice</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>ice</Name>
										</d2p1:anyType>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>false</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include />
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>Default</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs></MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.terrainmaterial</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\snow</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>snow</Name>
										</d2p1:anyType>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>true</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*_spc.tga</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*_nrm.tga</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>DXT1</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs></MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>true</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include />
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>DXT5</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs>(,,r)(,,g)(,,b)(_dif,_alp,r)</MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="GenericImageToDataTMMBurnSettings">
															<AlphaThreshold>50</AlphaThreshold>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include />
															</Metadata>
															<ReduceFactor>8</ReduceFactor>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\splats</RelativeName>
												</d2p1:anyType>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.terrainmaterial</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\splats</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>splats</Name>
										</d2p1:anyType>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>false</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include />
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>Default</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs></MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
														<d2p1:anyType i:type="CopyBurnSettings">
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include>
																	<BurnFolderMetadata.BurnFolderMetadataValue>
																		<Value>*.terrainmaterial</Value>
																	</BurnFolderMetadata.BurnFolderMetadataValue>
																</Include>
															</Metadata>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\tiles</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>tiles</Name>
										</d2p1:anyType>
										<d2p1:anyType i:type="Folder">
											<Children>
												<d2p1:anyType i:type="BurnFolder">
													<BurnSettings>
														<d2p1:anyType i:type="GenericImageToDataRGTBurnSettings">
															<AlphaEdge>false</AlphaEdge>
															<BlackBorder>false</BlackBorder>
															<CompressTextures>true</CompressTextures>
															<FlipImage>false</FlipImage>
															<ForceFormat>false</ForceFormat>
															<Metadata i:type="BurnFolderMetadata">
																<Exclude />
																<Include />
															</Metadata>
															<MipDrop>0</MipDrop>
															<MipMap>true</MipMap>
															<PreferredFormat>Default</PreferredFormat>
															<RescaleNonPowerTwo>true</RescaleNonPowerTwo>
															<TexSharpen>false</TexSharpen>
															<MixInputs></MixInputs>
															<MixInputsDefaults></MixInputsDefaults>
														</d2p1:anyType>
													</BurnSettings>
													<Hint>Default</Hint>
													<RelativeName>art\textures\water</RelativeName>
												</d2p1:anyType>
											</Children>
											<IsExpanded>false</IsExpanded>
											<Name>water</Name>
										</d2p1:anyType>
									</Children>
									<IsExpanded>false</IsExpanded>
									<Name>textures</Name>
								</d2p1:anyType>
							</Children>
							<IsExpanded>false</IsExpanded>
							<Name>scenarios</Name>
						</d2p1:anyType>
					</Children>
					<IsExpanded>false</IsExpanded>
					<Name>art</Name>
				</d2p1:anyType>
			</Children>
			<IsExpanded>false</IsExpanded>
		</d2p1:anyType>
	</Children>
	<Guid>f0c3cb74-0084-4726-8cad-2804e3b9d565</Guid>
	<IsExpanded>false</IsExpanded>
	<Type>AssetPack</Type>
</Document>